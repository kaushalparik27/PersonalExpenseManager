using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Dashboard;
using PersonalExpenseManager.Application.Dashboard.Dtos;
using PersonalExpenseManager.Application.Transactions;
using PersonalExpenseManager.Application.Transactions.Dtos;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class DashboardQueries(
    ApplicationDbContext db,
    IAccountQueries accountQueries,
    ITransactionQueries transactionQueries) : IDashboardQueries
{
    private const int TrendMonths = 6;
    private const int TopCategoryCount = 5;
    private const int RecentTransactionCount = 8;

    public async Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var prevMonthStart = monthStart.AddMonths(-1);
        var prevMonthEnd = monthStart.AddDays(-1);

        var accounts = await accountQueries.GetForUserAsync(userId, cancellationToken);
        var activeAccounts = accounts.Where(a => a.IsActive).ToList();
        var activeAccountIds = activeAccounts.Select(a => a.Id).ToHashSet();

        var totalBalance = activeAccounts.Sum(a => a.CurrentBalance);
        var balanceByType = activeAccounts
            .GroupBy(a => a.AccountType)
            .Select(g => new AccountTypeBalanceDto(g.Key, g.Sum(a => a.CurrentBalance)))
            .OrderByDescending(x => x.Balance)
            .ToList();

        var (thisMonthIncome, thisMonthExpense) = await GetIncomeExpenseTotalsAsync(userId, monthStart, monthEnd, activeAccountIds, cancellationToken);
        var (prevMonthIncome, prevMonthExpense) = await GetIncomeExpenseTotalsAsync(userId, prevMonthStart, prevMonthEnd, activeAccountIds, cancellationToken);

        var netCashFlow = thisMonthIncome - thisMonthExpense;

        // Handle zero denominators safely per spec: no rate to report, not zero.
        decimal? savingsRate = thisMonthIncome > 0
            ? Math.Round((thisMonthIncome - thisMonthExpense) / thisMonthIncome * 100, 1)
            : null;
        decimal? expenseGrowth = prevMonthExpense > 0
            ? Math.Round((thisMonthExpense - prevMonthExpense) / prevMonthExpense * 100, 1)
            : null;

        var topExpenseCategories = await GetTopExpenseCategoriesAsync(userId, monthStart, monthEnd, activeAccountIds, cancellationToken);
        var trendStart = monthStart.AddMonths(-(TrendMonths - 1));
        var trend = await MonthlySeriesHelper.GetMonthlyIncomeExpenseSeriesAsync(db, userId, trendStart, monthEnd, activeAccountIds, cancellationToken);
        var balanceTrend = await GetBalanceTrendAsync(userId, activeAccounts, trendStart, monthEnd, cancellationToken);

        var recentTransactionsPage = await transactionQueries.GetPagedAsync(
            userId,
            new TransactionListQuery(Page: 1, PageSize: RecentTransactionCount, SortBy: "transactionDate", SortDirection: "desc"),
            cancellationToken);

        var insights = BuildInsights(thisMonthIncome, prevMonthIncome, expenseGrowth, savingsRate, topExpenseCategories);

        return new DashboardDto(
            totalBalance,
            balanceByType,
            thisMonthIncome,
            thisMonthExpense,
            netCashFlow,
            savingsRate,
            expenseGrowth,
            topExpenseCategories,
            trend,
            balanceTrend,
            recentTransactionsPage.Items,
            accounts,
            insights);
    }

    private async Task<(decimal Income, decimal Expense)> GetIncomeExpenseTotalsAsync(
        Guid userId, DateOnly from, DateOnly to, IReadOnlyCollection<Guid> activeAccountIds, CancellationToken cancellationToken)
    {
        var totals = await db.Transactions
            .Where(t => t.UserId == userId
                && t.TransactionDate >= from && t.TransactionDate <= to
                && (t.TransactionType == TransactionType.Income || t.TransactionType == TransactionType.Expense)
                && activeAccountIds.Contains(t.AccountId))
            .GroupBy(t => t.TransactionType)
            .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var income = totals.FirstOrDefault(t => t.Type == TransactionType.Income)?.Total ?? 0m;
        var expense = totals.FirstOrDefault(t => t.Type == TransactionType.Expense)?.Total ?? 0m;
        return (income, expense);
    }

    // Balance is cumulative, so the trend's starting point needs everything that
    // ever happened on an active account, not just the six months being plotted —
    // unlike income/expense totals, which are naturally scoped to one period.
    private async Task<IReadOnlyList<BalanceTrendPointDto>> GetBalanceTrendAsync(
        Guid userId, IReadOnlyList<AccountDto> activeAccounts, DateOnly trendStart, DateOnly trendEnd, CancellationToken cancellationToken)
    {
        if (activeAccounts.Count == 0)
        {
            return [];
        }

        var activeAccountIds = activeAccounts.Select(a => a.Id).ToHashSet();
        var openingTotal = activeAccounts.Sum(a => a.OpeningBalance);

        var baselineDelta = await GetNetBalanceDeltaAsync(userId, activeAccountIds, before: trendStart, cancellationToken);
        var monthlyDeltas = await GetMonthlyNetBalanceDeltasAsync(userId, activeAccountIds, trendStart, trendEnd, cancellationToken);

        var runningBalance = openingTotal + baselineDelta;
        var result = new List<BalanceTrendPointDto>();
        for (var month = trendStart; month <= trendEnd; month = month.AddMonths(1))
        {
            runningBalance += monthlyDeltas.GetValueOrDefault((month.Year, month.Month), 0m);
            result.Add(new BalanceTrendPointDto(month.Year, month.Month, runningBalance));
        }

        return result;
    }

    private async Task<decimal> GetNetBalanceDeltaAsync(
        Guid userId, ISet<Guid> accountIds, DateOnly before, CancellationToken cancellationToken)
    {
        var byType = await db.Transactions
            .Where(t => t.UserId == userId && accountIds.Contains(t.AccountId) && t.TransactionDate < before)
            .GroupBy(t => t.TransactionType)
            .Select(g => new { g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(g => g.Key, g => g.Total, cancellationToken);

        var transferIn = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Transfer
                && t.TransferToAccountId != null && accountIds.Contains(t.TransferToAccountId!.Value)
                && t.TransactionDate < before)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        return NetBalanceDelta(byType, transferIn);
    }

    private async Task<Dictionary<(int Year, int Month), decimal>> GetMonthlyNetBalanceDeltasAsync(
        Guid userId, ISet<Guid> accountIds, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var byTypeMonthly = await db.Transactions
            .Where(t => t.UserId == userId && accountIds.Contains(t.AccountId)
                && t.TransactionDate >= from && t.TransactionDate <= to)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month, t.TransactionType })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.TransactionType, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var transferInMonthly = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Transfer
                && t.TransferToAccountId != null && accountIds.Contains(t.TransferToAccountId!.Value)
                && t.TransactionDate >= from && t.TransactionDate <= to)
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var months = byTypeMonthly.Select(r => (r.Year, r.Month))
            .Concat(transferInMonthly.Select(r => (r.Year, r.Month)))
            .Distinct();

        var result = new Dictionary<(int, int), decimal>();
        foreach (var (year, month) in months)
        {
            var byType = byTypeMonthly.Where(r => r.Year == year && r.Month == month)
                .ToDictionary(r => r.TransactionType, r => r.Total);
            var transferIn = transferInMonthly.Where(r => r.Year == year && r.Month == month).Sum(r => r.Total);
            result[(year, month)] = NetBalanceDelta(byType, transferIn);
        }

        return result;
    }

    private static decimal NetBalanceDelta(IReadOnlyDictionary<TransactionType, decimal> byType, decimal transferIn) =>
        byType.GetValueOrDefault(TransactionType.Income) - byType.GetValueOrDefault(TransactionType.Expense)
        + transferIn - byType.GetValueOrDefault(TransactionType.Transfer) + byType.GetValueOrDefault(TransactionType.Adjustment);

    private async Task<IReadOnlyList<CategoryAmountDto>> GetTopExpenseCategoriesAsync(
        Guid userId, DateOnly from, DateOnly to, IReadOnlyCollection<Guid> activeAccountIds, CancellationToken cancellationToken)
    {
        var grouped = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Expense
                && t.TransactionDate >= from && t.TransactionDate <= to && t.ExpenseCategoryId != null
                && activeAccountIds.Contains(t.AccountId))
            .GroupBy(t => t.ExpenseCategoryId)
            .Select(g => new { CategoryId = g.Key!.Value, Total = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Total)
            .Take(TopCategoryCount)
            .ToListAsync(cancellationToken);

        if (grouped.Count == 0)
        {
            return [];
        }

        var categoryIds = grouped.Select(g => g.CategoryId).ToList();
        var names = await db.ExpenseCategories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        return grouped
            .Select(g => new CategoryAmountDto(names.GetValueOrDefault(g.CategoryId, "Unknown"), g.Total))
            .ToList();
    }

    private static IReadOnlyList<string> BuildInsights(
        decimal thisMonthIncome,
        decimal prevMonthIncome,
        decimal? expenseGrowth,
        decimal? savingsRate,
        IReadOnlyList<CategoryAmountDto> topExpenseCategories)
    {
        var insights = new List<string>();

        if (expenseGrowth is { } growth)
        {
            insights.Add(growth <= 0
                ? $"You spent {Math.Abs(growth):0.#}% less than last month."
                : $"You spent {growth:0.#}% more than last month.");
        }

        if (topExpenseCategories.Count > 0)
        {
            insights.Add($"{topExpenseCategories[0].CategoryName} is your largest expense category.");
        }

        if (prevMonthIncome > 0 && thisMonthIncome != prevMonthIncome)
        {
            insights.Add(thisMonthIncome > prevMonthIncome
                ? "Income increased compared with last month."
                : "Income decreased compared with last month.");
        }

        if (savingsRate is { } rate)
        {
            insights.Add($"Your savings rate is {rate:0.#}%.");
        }

        if (insights.Count == 0)
        {
            insights.Add("Not enough data yet.");
        }

        return insights;
    }
}
