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

    // Computes every period's totals up front rather than accepting a selection
    // param — the Summary section lets the user pick 1-6 of these to display,
    // and picking client-side avoids a bespoke multi-value query-string API.
    public async Task<IReadOnlyList<PeriodSummaryDto>> GetPeriodSummariesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await accountQueries.GetForUserAsync(userId, cancellationToken);
        var activeAccountIds = accounts.Where(a => a.IsActive).Select(a => a.Id).ToHashSet();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var prevMonthStart = monthStart.AddMonths(-1);
        var prevMonthEnd = monthStart.AddDays(-1);
        var yearStart = new DateOnly(today.Year, 1, 1);
        var yearEnd = new DateOnly(today.Year, 12, 31);
        var prevYearStart = new DateOnly(today.Year - 1, 1, 1);
        var prevYearEnd = new DateOnly(today.Year - 1, 12, 31);

        var ranges = new (SummaryPeriod Period, DateOnly From, DateOnly To)[]
        {
            (SummaryPeriod.Today, today, today),
            (SummaryPeriod.Yesterday, today.AddDays(-1), today.AddDays(-1)),
            (SummaryPeriod.Last7Days, today.AddDays(-6), today),
            (SummaryPeriod.ThisMonth, monthStart, monthEnd),
            (SummaryPeriod.Last31Days, today.AddDays(-30), today),
            (SummaryPeriod.LastMonth, prevMonthStart, prevMonthEnd),
            (SummaryPeriod.ThisYear, yearStart, yearEnd),
            (SummaryPeriod.LastYear, prevYearStart, prevYearEnd),
            (SummaryPeriod.Total, DateOnly.MinValue, DateOnly.MaxValue),
        };

        var result = new List<PeriodSummaryDto>();
        foreach (var (period, from, to) in ranges)
        {
            var (income, expense) = await GetIncomeExpenseTotalsAsync(userId, from, to, activeAccountIds, cancellationToken);
            result.Add(new PeriodSummaryDto(period, income, expense));
        }

        return result;
    }

    // Balance is cumulative, so the trend's starting point needs everything that
    // ever happened on an active account, not just the buckets being plotted —
    // unlike income/expense totals, which are naturally scoped to one period.
    // Buckets are built at day granularity server-side regardless of the
    // requested view, then aggregated per bucket — this way every granularity
    // (day/week/fortnight/month/year) shares one query shape instead of five.
    public async Task<IReadOnlyList<BalanceTrendPointDto>> GetBalanceTrendAsync(
        Guid userId, BalanceTrendGranularity granularity, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var accounts = await accountQueries.GetForUserAsync(userId, cancellationToken);
        var activeAccounts = accounts.Where(a => a.IsActive).ToList();

        if (activeAccounts.Count == 0)
        {
            return [];
        }

        var buckets = BuildBalanceTrendBuckets(granularity, today);
        var activeAccountIds = activeAccounts.Select(a => a.Id).ToHashSet();
        var openingTotal = activeAccounts.Sum(a => a.OpeningBalance);

        var windowStart = buckets[0].Start;
        var baselineDelta = await GetNetBalanceDeltaAsync(userId, activeAccountIds, before: windowStart, cancellationToken);
        var dailyDeltas = await GetDailyNetBalanceDeltasAsync(userId, activeAccountIds, windowStart, today, cancellationToken);

        var runningBalance = openingTotal + baselineDelta;
        var result = new List<BalanceTrendPointDto>();
        foreach (var (start, end) in buckets)
        {
            var bucketDelta = dailyDeltas.Where(kv => kv.Key >= start && kv.Key <= end).Sum(kv => kv.Value);
            runningBalance += bucketDelta;
            result.Add(new BalanceTrendPointDto(start, runningBalance));
        }

        return result;
    }

    private static IReadOnlyList<(DateOnly Start, DateOnly End)> BuildBalanceTrendBuckets(BalanceTrendGranularity granularity, DateOnly today) =>
        granularity switch
        {
            BalanceTrendGranularity.Daily => RollingBuckets(today, stepDays: 1, count: 30),
            BalanceTrendGranularity.Weekly => RollingBuckets(today, stepDays: 7, count: 12),
            BalanceTrendGranularity.BiWeekly => RollingBuckets(today, stepDays: 14, count: 12),
            BalanceTrendGranularity.Monthly => MonthlyBuckets(today, count: 12),
            BalanceTrendGranularity.Yearly => YearlyBuckets(today, count: 5),
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, "Unhandled balance trend granularity."),
        };

    // Rolling, fixed-size windows ending today — used for Daily/Weekly/BiWeekly
    // since those don't have an established calendar-aligned convention the way
    // months and years do (no canonical "start of fortnight").
    private static List<(DateOnly Start, DateOnly End)> RollingBuckets(DateOnly today, int stepDays, int count)
    {
        var buckets = new List<(DateOnly, DateOnly)>();
        for (var i = count - 1; i >= 0; i--)
        {
            var end = today.AddDays(-i * stepDays);
            var start = end.AddDays(-(stepDays - 1));
            buckets.Add((start, end));
        }

        return buckets;
    }

    private static List<(DateOnly Start, DateOnly End)> MonthlyBuckets(DateOnly today, int count)
    {
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var start = monthStart.AddMonths(-(count - 1));

        var buckets = new List<(DateOnly, DateOnly)>();
        for (var m = start; m <= monthStart; m = m.AddMonths(1))
        {
            buckets.Add((m, m.AddMonths(1).AddDays(-1)));
        }

        return buckets;
    }

    private static List<(DateOnly Start, DateOnly End)> YearlyBuckets(DateOnly today, int count)
    {
        var yearStart = new DateOnly(today.Year, 1, 1);
        var start = new DateOnly(today.Year - (count - 1), 1, 1);

        var buckets = new List<(DateOnly, DateOnly)>();
        for (var y = start; y <= yearStart; y = y.AddYears(1))
        {
            buckets.Add((y, y.AddYears(1).AddDays(-1)));
        }

        return buckets;
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

    private async Task<Dictionary<DateOnly, decimal>> GetDailyNetBalanceDeltasAsync(
        Guid userId, ISet<Guid> accountIds, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var byTypeDaily = await db.Transactions
            .Where(t => t.UserId == userId && accountIds.Contains(t.AccountId)
                && t.TransactionDate >= from && t.TransactionDate <= to)
            .GroupBy(t => new { t.TransactionDate, t.TransactionType })
            .Select(g => new { g.Key.TransactionDate, g.Key.TransactionType, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var transferInDaily = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Transfer
                && t.TransferToAccountId != null && accountIds.Contains(t.TransferToAccountId!.Value)
                && t.TransactionDate >= from && t.TransactionDate <= to)
            .GroupBy(t => t.TransactionDate)
            .Select(g => new { Date = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var dates = byTypeDaily.Select(r => r.TransactionDate)
            .Concat(transferInDaily.Select(r => r.Date))
            .Distinct();

        var result = new Dictionary<DateOnly, decimal>();
        foreach (var date in dates)
        {
            var byType = byTypeDaily.Where(r => r.TransactionDate == date).ToDictionary(r => r.TransactionType, r => r.Total);
            var transferIn = transferInDaily.Where(r => r.Date == date).Sum(r => r.Total);
            result[date] = NetBalanceDelta(byType, transferIn);
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
