using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.RecurringTransactions;
using PersonalExpenseManager.Application.RecurringTransactions.Dtos;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Infrastructure.Persistence.Commands;

public class RecurringTransactionCommands(ApplicationDbContext db, IRecurringTransactionQueries queries) : IRecurringTransactionCommands
{
    public async Task<MutationResult<RecurringTransactionDto>> CreateExpenseAsync(Guid userId, CreateRecurringExpenseRequest request, CancellationToken cancellationToken = default)
    {
        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<RecurringTransactionDto>.Fail("Account not found.");
        }

        if (!await ExpenseCategoryValidAsync(userId, request.ExpenseCategoryId, request.ExpenseSubCategoryId, cancellationToken))
        {
            return MutationResult<RecurringTransactionDto>.Fail("Category not found.");
        }

        var frequency = ParseFrequency(request.Frequency);

        var recurring = new RecurringTransaction
        {
            UserId = userId,
            TransactionType = TransactionType.Expense,
            AccountId = request.AccountId,
            Amount = request.Amount,
            ExpenseCategoryId = request.ExpenseCategoryId,
            ExpenseSubCategoryId = request.ExpenseSubCategoryId,
            Description = request.Description,
            Frequency = frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextOccurrenceDate = request.StartDate,
        };

        db.RecurringTransactions.Add(recurring);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<RecurringTransactionDto>.Ok((await queries.GetByIdAsync(userId, recurring.Id, cancellationToken))!);
    }

    public async Task<MutationResult<RecurringTransactionDto>> CreateIncomeAsync(Guid userId, CreateRecurringIncomeRequest request, CancellationToken cancellationToken = default)
    {
        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<RecurringTransactionDto>.Fail("Account not found.");
        }

        if (!await IncomeCategoryValidAsync(userId, request.IncomeCategoryId, request.IncomeSubCategoryId, cancellationToken))
        {
            return MutationResult<RecurringTransactionDto>.Fail("Category not found.");
        }

        var frequency = ParseFrequency(request.Frequency);

        var recurring = new RecurringTransaction
        {
            UserId = userId,
            TransactionType = TransactionType.Income,
            AccountId = request.AccountId,
            Amount = request.Amount,
            IncomeCategoryId = request.IncomeCategoryId,
            IncomeSubCategoryId = request.IncomeSubCategoryId,
            Description = request.Description,
            Frequency = frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextOccurrenceDate = request.StartDate,
        };

        db.RecurringTransactions.Add(recurring);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<RecurringTransactionDto>.Ok((await queries.GetByIdAsync(userId, recurring.Id, cancellationToken))!);
    }

    public async Task<MutationResult<RecurringTransactionDto>> CreateTransferAsync(Guid userId, CreateRecurringTransferRequest request, CancellationToken cancellationToken = default)
    {
        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<RecurringTransactionDto>.Fail("Source account not found.");
        }

        if (!await AccountBelongsToUserAsync(userId, request.TransferToAccountId, cancellationToken))
        {
            return MutationResult<RecurringTransactionDto>.Fail("Destination account not found.");
        }

        var frequency = ParseFrequency(request.Frequency);

        var recurring = new RecurringTransaction
        {
            UserId = userId,
            TransactionType = TransactionType.Transfer,
            AccountId = request.AccountId,
            TransferToAccountId = request.TransferToAccountId,
            Amount = request.Amount,
            Description = request.Description,
            Frequency = frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextOccurrenceDate = request.StartDate,
        };

        db.RecurringTransactions.Add(recurring);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<RecurringTransactionDto>.Ok((await queries.GetByIdAsync(userId, recurring.Id, cancellationToken))!);
    }

    public async Task<MutationResult<RecurringTransactionDto>> UpdateAsync(Guid userId, Guid recurringTransactionId, UpdateRecurringTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var recurring = await db.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == recurringTransactionId && r.UserId == userId, cancellationToken);

        if (recurring is null)
        {
            return MutationResult<RecurringTransactionDto>.NotFound();
        }

        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<RecurringTransactionDto>.Fail("Account not found.");
        }

        switch (recurring.TransactionType)
        {
            case TransactionType.Expense:
                if (request.ExpenseCategoryId is null || request.IncomeCategoryId is not null
                    || request.IncomeSubCategoryId is not null || request.TransferToAccountId is not null)
                {
                    return MutationResult<RecurringTransactionDto>.Fail("An expense must have an expense category and no income or transfer fields.");
                }

                if (!await ExpenseCategoryValidAsync(userId, request.ExpenseCategoryId.Value, request.ExpenseSubCategoryId, cancellationToken))
                {
                    return MutationResult<RecurringTransactionDto>.Fail("Category not found.");
                }

                break;

            case TransactionType.Income:
                if (request.IncomeCategoryId is null || request.ExpenseCategoryId is not null
                    || request.ExpenseSubCategoryId is not null || request.TransferToAccountId is not null)
                {
                    return MutationResult<RecurringTransactionDto>.Fail("Income must have an income category and no expense or transfer fields.");
                }

                if (!await IncomeCategoryValidAsync(userId, request.IncomeCategoryId.Value, request.IncomeSubCategoryId, cancellationToken))
                {
                    return MutationResult<RecurringTransactionDto>.Fail("Category not found.");
                }

                break;

            case TransactionType.Transfer:
                if (request.TransferToAccountId is null || request.ExpenseCategoryId is not null || request.IncomeCategoryId is not null)
                {
                    return MutationResult<RecurringTransactionDto>.Fail("A transfer must have a destination account and no category fields.");
                }

                if (request.TransferToAccountId == request.AccountId)
                {
                    return MutationResult<RecurringTransactionDto>.Fail("Cannot transfer an account to itself.");
                }

                if (!await AccountBelongsToUserAsync(userId, request.TransferToAccountId.Value, cancellationToken))
                {
                    return MutationResult<RecurringTransactionDto>.Fail("Destination account not found.");
                }

                break;
        }

        var frequency = ParseFrequency(request.Frequency);

        recurring.AccountId = request.AccountId;
        recurring.TransferToAccountId = request.TransferToAccountId;
        recurring.Amount = request.Amount;
        recurring.ExpenseCategoryId = request.ExpenseCategoryId;
        recurring.ExpenseSubCategoryId = request.ExpenseSubCategoryId;
        recurring.IncomeCategoryId = request.IncomeCategoryId;
        recurring.IncomeSubCategoryId = request.IncomeSubCategoryId;
        recurring.Description = request.Description;
        recurring.Frequency = frequency;
        recurring.StartDate = request.StartDate;
        recurring.EndDate = request.EndDate;

        // The engine has already advanced NextOccurrenceDate past any generated
        // occurrences; only pull it forward if the new start date is later, never
        // rewind it — rewinding would let already-generated dates be regenerated.
        if (request.StartDate > recurring.NextOccurrenceDate)
        {
            recurring.NextOccurrenceDate = request.StartDate;
        }

        recurring.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<RecurringTransactionDto>.Ok((await queries.GetByIdAsync(userId, recurring.Id, cancellationToken))!);
    }

    public async Task<MutationResult<RecurringTransactionDto>> SetActiveAsync(Guid userId, Guid recurringTransactionId, bool isActive, CancellationToken cancellationToken = default)
    {
        var recurring = await db.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == recurringTransactionId && r.UserId == userId, cancellationToken);

        if (recurring is null)
        {
            return MutationResult<RecurringTransactionDto>.NotFound();
        }

        recurring.IsActive = isActive;
        recurring.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<RecurringTransactionDto>.Ok((await queries.GetByIdAsync(userId, recurring.Id, cancellationToken))!);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid recurringTransactionId, CancellationToken cancellationToken = default)
    {
        var recurring = await db.RecurringTransactions
            .FirstOrDefaultAsync(r => r.Id == recurringTransactionId && r.UserId == userId, cancellationToken);

        if (recurring is null)
        {
            return false;
        }

        // Hard delete: unlike Transaction, a RecurringTransaction is a template, not
        // financial history. Transactions it already generated keep their own history
        // (Transaction.RecurringTransactionId is SetNull on delete, see TransactionConfiguration).
        db.RecurringTransactions.Remove(recurring);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static RecurrenceFrequency ParseFrequency(string frequency) =>
        Enum.Parse<RecurrenceFrequency>(frequency, ignoreCase: true);

    private Task<bool> AccountBelongsToUserAsync(Guid userId, Guid accountId, CancellationToken cancellationToken) =>
        db.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);

    private async Task<bool> ExpenseCategoryValidAsync(Guid userId, Guid categoryId, Guid? subCategoryId, CancellationToken cancellationToken)
    {
        if (!await db.ExpenseCategories.AnyAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken))
        {
            return false;
        }

        return subCategoryId is null
            || await db.ExpenseSubCategories.AnyAsync(s => s.Id == subCategoryId && s.UserId == userId && s.ExpenseCategoryId == categoryId, cancellationToken);
    }

    private async Task<bool> IncomeCategoryValidAsync(Guid userId, Guid categoryId, Guid? subCategoryId, CancellationToken cancellationToken)
    {
        if (!await db.IncomeCategories.AnyAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken))
        {
            return false;
        }

        return subCategoryId is null
            || await db.IncomeSubCategories.AnyAsync(s => s.Id == subCategoryId && s.UserId == userId && s.IncomeCategoryId == categoryId, cancellationToken);
    }
}
