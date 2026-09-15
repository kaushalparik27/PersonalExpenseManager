using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Dashboard.Dtos;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

// Shared by DashboardQueries (fixed trailing window) and ReportsQueries
// (arbitrary user-selected range) so the month-bucketing logic — including
// filling in zero-value months with no transactions — exists in one place.
internal static class MonthlySeriesHelper
{
    public static async Task<IReadOnlyList<MonthlyAmountDto>> GetMonthlyIncomeExpenseSeriesAsync(
        ApplicationDbContext db, Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken)
    {
        var rows = await db.Transactions
            .Where(t => t.UserId == userId && t.TransactionDate >= fromDate && t.TransactionDate <= toDate
                && (t.TransactionType == TransactionType.Income || t.TransactionType == TransactionType.Expense))
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month, t.TransactionType })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.TransactionType, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var result = new List<MonthlyAmountDto>();
        var current = new DateOnly(fromDate.Year, fromDate.Month, 1);
        var end = new DateOnly(toDate.Year, toDate.Month, 1);

        while (current <= end)
        {
            var income = rows.FirstOrDefault(r => r.Year == current.Year && r.Month == current.Month && r.TransactionType == TransactionType.Income)?.Total ?? 0m;
            var expense = rows.FirstOrDefault(r => r.Year == current.Year && r.Month == current.Month && r.TransactionType == TransactionType.Expense)?.Total ?? 0m;
            result.Add(new MonthlyAmountDto(current.Year, current.Month, income, expense));
            current = current.AddMonths(1);
        }

        return result;
    }
}
