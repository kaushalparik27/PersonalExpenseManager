using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Dashboard.Dtos;
using PersonalExpenseManager.Application.Reports;
using PersonalExpenseManager.Application.Reports.Dtos;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class ReportsQueries(ApplicationDbContext db, IAccountQueries accountQueries) : IReportsQueries
{
    public async Task<IncomeExpenseReportDto> GetIncomeReportAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var series = await MonthlySeriesHelper.GetMonthlyIncomeExpenseSeriesAsync(db, userId, fromDate, toDate, cancellationToken);
        var categories = await GetCategoryAmountsAsync(userId, TransactionType.Income, fromDate, toDate, cancellationToken);

        return new IncomeExpenseReportDto(series.Sum(s => s.Income), series, categories);
    }

    public async Task<IncomeExpenseReportDto> GetExpenseReportAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var series = await MonthlySeriesHelper.GetMonthlyIncomeExpenseSeriesAsync(db, userId, fromDate, toDate, cancellationToken);
        var categories = await GetCategoryAmountsAsync(userId, TransactionType.Expense, fromDate, toDate, cancellationToken);

        return new IncomeExpenseReportDto(series.Sum(s => s.Expense), series, categories);
    }

    public async Task<CashFlowReportDto> GetCashFlowReportAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var series = await MonthlySeriesHelper.GetMonthlyIncomeExpenseSeriesAsync(db, userId, fromDate, toDate, cancellationToken);
        var totalIncome = series.Sum(s => s.Income);
        var totalExpense = series.Sum(s => s.Expense);

        decimal? savingsRate = totalIncome > 0
            ? Math.Round((totalIncome - totalExpense) / totalIncome * 100, 1)
            : null;

        return new CashFlowReportDto(totalIncome, totalExpense, totalIncome - totalExpense, savingsRate, series);
    }

    public async Task<CategorySummaryReportDto> GetCategorySummaryAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var expenseCategories = await BuildExpenseCategorySummaryAsync(userId, fromDate, toDate, cancellationToken);
        var incomeCategories = await BuildIncomeCategorySummaryAsync(userId, fromDate, toDate, cancellationToken);

        return new CategorySummaryReportDto(expenseCategories, incomeCategories);
    }

    public async Task<AccountSummaryReportDto> GetAccountSummaryAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var accounts = await accountQueries.GetForUserAsync(userId, cancellationToken);

        var byAccountAndType = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
            .GroupBy(t => new { t.AccountId, t.TransactionType })
            .Select(g => new { g.Key.AccountId, g.Key.TransactionType, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var transfersIn = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Transfer && t.TransferToAccountId != null
                && t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
            .GroupBy(t => t.TransferToAccountId!.Value)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        decimal SumFor(Guid accountId, TransactionType type) =>
            byAccountAndType.Where(r => r.AccountId == accountId && r.TransactionType == type).Sum(r => r.Total);

        var items = accounts.Select(a =>
        {
            var income = SumFor(a.Id, TransactionType.Income);
            var expense = SumFor(a.Id, TransactionType.Expense);
            var transferOut = SumFor(a.Id, TransactionType.Transfer);
            var transferIn = transfersIn.Where(t => t.AccountId == a.Id).Sum(t => t.Total);
            var adjustments = SumFor(a.Id, TransactionType.Adjustment);
            var netChange = income - expense + transferIn - transferOut + adjustments;

            return new AccountSummaryItemDto(a.Id, a.Name, a.AccountType, income, expense, transferIn, transferOut, netChange, a.CurrentBalance);
        }).ToList();

        return new AccountSummaryReportDto(items);
    }

    public async Task<MonthlyTrendReportDto> GetMonthlyTrendAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var series = await MonthlySeriesHelper.GetMonthlyIncomeExpenseSeriesAsync(db, userId, fromDate, toDate, cancellationToken);
        return new MonthlyTrendReportDto(series);
    }

    private async Task<IReadOnlyList<CategoryAmountDto>> GetCategoryAmountsAsync(
        Guid userId, TransactionType type, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        if (type == TransactionType.Expense)
        {
            var grouped = await db.Transactions
                .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Expense
                    && t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.ExpenseCategoryId != null)
                .GroupBy(t => t.ExpenseCategoryId)
                .Select(g => new { CategoryId = g.Key!.Value, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync(cancellationToken);

            if (grouped.Count == 0)
            {
                return [];
            }

            var categoryIds = grouped.Select(g => g.CategoryId).ToList();
            var names = await db.ExpenseCategories
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

            return grouped.Select(g => new CategoryAmountDto(names.GetValueOrDefault(g.CategoryId, "Unknown"), g.Total)).ToList();
        }
        else
        {
            var grouped = await db.Transactions
                .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Income
                    && t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.IncomeCategoryId != null)
                .GroupBy(t => t.IncomeCategoryId)
                .Select(g => new { CategoryId = g.Key!.Value, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync(cancellationToken);

            if (grouped.Count == 0)
            {
                return [];
            }

            var categoryIds = grouped.Select(g => g.CategoryId).ToList();
            var names = await db.IncomeCategories
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

            return grouped.Select(g => new CategoryAmountDto(names.GetValueOrDefault(g.CategoryId, "Unknown"), g.Total)).ToList();
        }
    }

    private async Task<IReadOnlyList<CategorySummaryCategoryDto>> BuildExpenseCategorySummaryAsync(
        Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var categoryTotals = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Expense
                && t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.ExpenseCategoryId != null)
            .GroupBy(t => t.ExpenseCategoryId)
            .Select(g => new { CategoryId = g.Key!.Value, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        if (categoryTotals.Count == 0)
        {
            return [];
        }

        var subCategoryTotals = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Expense
                && t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.ExpenseSubCategoryId != null)
            .GroupBy(t => t.ExpenseSubCategoryId)
            .Select(g => new { SubCategoryId = g.Key!.Value, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(g => g.SubCategoryId, g => g.Total, cancellationToken);

        var categoryIds = categoryTotals.Select(c => c.CategoryId).ToList();
        var categories = await db.ExpenseCategories
            .Where(c => categoryIds.Contains(c.Id))
            .Include(c => c.SubCategories)
            .ToListAsync(cancellationToken);

        return categoryTotals
            .Select(ct =>
            {
                var category = categories.First(c => c.Id == ct.CategoryId);
                var subCategories = category.SubCategories
                    .Where(s => subCategoryTotals.ContainsKey(s.Id))
                    .Select(s => new CategorySummarySubCategoryDto(s.Id, s.Name, subCategoryTotals[s.Id]))
                    .OrderByDescending(s => s.Amount)
                    .ToList();

                return new CategorySummaryCategoryDto(category.Id, category.Name, ct.Total, subCategories);
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    private async Task<IReadOnlyList<CategorySummaryCategoryDto>> BuildIncomeCategorySummaryAsync(
        Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var categoryTotals = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Income
                && t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.IncomeCategoryId != null)
            .GroupBy(t => t.IncomeCategoryId)
            .Select(g => new { CategoryId = g.Key!.Value, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        if (categoryTotals.Count == 0)
        {
            return [];
        }

        var subCategoryTotals = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Income
                && t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.IncomeSubCategoryId != null)
            .GroupBy(t => t.IncomeSubCategoryId)
            .Select(g => new { SubCategoryId = g.Key!.Value, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(g => g.SubCategoryId, g => g.Total, cancellationToken);

        var categoryIds = categoryTotals.Select(c => c.CategoryId).ToList();
        var categories = await db.IncomeCategories
            .Where(c => categoryIds.Contains(c.Id))
            .Include(c => c.SubCategories)
            .ToListAsync(cancellationToken);

        return categoryTotals
            .Select(ct =>
            {
                var category = categories.First(c => c.Id == ct.CategoryId);
                var subCategories = category.SubCategories
                    .Where(s => subCategoryTotals.ContainsKey(s.Id))
                    .Select(s => new CategorySummarySubCategoryDto(s.Id, s.Name, subCategoryTotals[s.Id]))
                    .OrderByDescending(s => s.Amount)
                    .ToList();

                return new CategorySummaryCategoryDto(category.Id, category.Name, ct.Total, subCategories);
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }
}
