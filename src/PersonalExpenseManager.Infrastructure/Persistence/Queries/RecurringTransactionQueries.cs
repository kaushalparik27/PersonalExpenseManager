using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.RecurringTransactions;
using PersonalExpenseManager.Application.RecurringTransactions.Dtos;
using PersonalExpenseManager.Domain.Entities;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class RecurringTransactionQueries(ApplicationDbContext db) : IRecurringTransactionQueries
{
    public async Task<IReadOnlyList<RecurringTransactionDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var recurring = await db.RecurringTransactions.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.NextOccurrenceDate)
            .ToListAsync(cancellationToken);

        return await MapToDtosAsync(recurring, cancellationToken);
    }

    public async Task<RecurringTransactionDto?> GetByIdAsync(Guid userId, Guid recurringTransactionId, CancellationToken cancellationToken = default)
    {
        var recurring = await db.RecurringTransactions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recurringTransactionId && r.UserId == userId, cancellationToken);

        if (recurring is null)
        {
            return null;
        }

        var dtos = await MapToDtosAsync([recurring], cancellationToken);
        return dtos[0];
    }

    private async Task<List<RecurringTransactionDto>> MapToDtosAsync(IReadOnlyList<RecurringTransaction> recurring, CancellationToken cancellationToken)
    {
        var accountIds = recurring
            .SelectMany(r => new[] { r.AccountId, r.TransferToAccountId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var expenseCategoryIds = recurring.Where(r => r.ExpenseCategoryId.HasValue).Select(r => r.ExpenseCategoryId!.Value).Distinct().ToList();
        var expenseSubCategoryIds = recurring.Where(r => r.ExpenseSubCategoryId.HasValue).Select(r => r.ExpenseSubCategoryId!.Value).Distinct().ToList();
        var incomeCategoryIds = recurring.Where(r => r.IncomeCategoryId.HasValue).Select(r => r.IncomeCategoryId!.Value).Distinct().ToList();
        var incomeSubCategoryIds = recurring.Where(r => r.IncomeSubCategoryId.HasValue).Select(r => r.IncomeSubCategoryId!.Value).Distinct().ToList();

        var accountNames = await db.Accounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

        var expenseCategoryNames = await db.ExpenseCategories.AsNoTracking()
            .Where(c => expenseCategoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var expenseSubCategoryNames = await db.ExpenseSubCategories.AsNoTracking()
            .Where(s => expenseSubCategoryIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var incomeCategoryNames = await db.IncomeCategories.AsNoTracking()
            .Where(c => incomeCategoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var incomeSubCategoryNames = await db.IncomeSubCategories.AsNoTracking()
            .Where(s => incomeSubCategoryIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        return recurring.Select(r => new RecurringTransactionDto(
            r.Id,
            r.TransactionType.ToString(),
            r.AccountId,
            accountNames.GetValueOrDefault(r.AccountId, "Unknown Account"),
            r.TransferToAccountId,
            r.TransferToAccountId.HasValue ? accountNames.GetValueOrDefault(r.TransferToAccountId.Value) : null,
            r.Amount,
            r.ExpenseCategoryId,
            r.ExpenseCategoryId.HasValue ? expenseCategoryNames.GetValueOrDefault(r.ExpenseCategoryId.Value) : null,
            r.ExpenseSubCategoryId,
            r.ExpenseSubCategoryId.HasValue ? expenseSubCategoryNames.GetValueOrDefault(r.ExpenseSubCategoryId.Value) : null,
            r.IncomeCategoryId,
            r.IncomeCategoryId.HasValue ? incomeCategoryNames.GetValueOrDefault(r.IncomeCategoryId.Value) : null,
            r.IncomeSubCategoryId,
            r.IncomeSubCategoryId.HasValue ? incomeSubCategoryNames.GetValueOrDefault(r.IncomeSubCategoryId.Value) : null,
            r.Description,
            r.Frequency.ToString(),
            r.StartDate,
            r.EndDate,
            r.NextOccurrenceDate,
            r.IsActive))
            .ToList();
    }
}
