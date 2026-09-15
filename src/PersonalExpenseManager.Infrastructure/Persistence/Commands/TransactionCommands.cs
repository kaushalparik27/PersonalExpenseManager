using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Transactions;
using PersonalExpenseManager.Application.Transactions.Dtos;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Infrastructure.Persistence.Commands;

public class TransactionCommands(ApplicationDbContext db, ITransactionQueries queries) : ITransactionCommands
{
    public async Task<MutationResult<TransactionDto>> CreateExpenseAsync(Guid userId, CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<TransactionDto>.Fail("Account not found.");
        }

        if (!await ExpenseCategoryValidAsync(userId, request.ExpenseCategoryId, request.ExpenseSubCategoryId, cancellationToken))
        {
            return MutationResult<TransactionDto>.Fail("Category not found.");
        }

        var transaction = new Transaction
        {
            UserId = userId,
            TransactionDate = request.TransactionDate,
            TransactionType = TransactionType.Expense,
            AccountId = request.AccountId,
            Amount = request.Amount,
            ExpenseCategoryId = request.ExpenseCategoryId,
            ExpenseSubCategoryId = request.ExpenseSubCategoryId,
            Description = request.Description,
            Notes = request.Notes,
            Payee = request.Payee,
            ReferenceNumber = request.ReferenceNumber,
        };

        await AttachTagsAsync(userId, transaction, request.Tags, cancellationToken);
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<TransactionDto>.Ok((await queries.GetByIdAsync(userId, transaction.Id, cancellationToken))!);
    }

    public async Task<MutationResult<TransactionDto>> CreateIncomeAsync(Guid userId, CreateIncomeRequest request, CancellationToken cancellationToken = default)
    {
        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<TransactionDto>.Fail("Account not found.");
        }

        if (!await IncomeCategoryValidAsync(userId, request.IncomeCategoryId, request.IncomeSubCategoryId, cancellationToken))
        {
            return MutationResult<TransactionDto>.Fail("Category not found.");
        }

        var transaction = new Transaction
        {
            UserId = userId,
            TransactionDate = request.TransactionDate,
            TransactionType = TransactionType.Income,
            AccountId = request.AccountId,
            Amount = request.Amount,
            IncomeCategoryId = request.IncomeCategoryId,
            IncomeSubCategoryId = request.IncomeSubCategoryId,
            Description = request.Description,
            Notes = request.Notes,
            Payee = request.Payee,
            ReferenceNumber = request.ReferenceNumber,
        };

        await AttachTagsAsync(userId, transaction, request.Tags, cancellationToken);
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<TransactionDto>.Ok((await queries.GetByIdAsync(userId, transaction.Id, cancellationToken))!);
    }

    public async Task<MutationResult<TransactionDto>> CreateTransferAsync(Guid userId, CreateTransferRequest request, CancellationToken cancellationToken = default)
    {
        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<TransactionDto>.Fail("Source account not found.");
        }

        if (!await AccountBelongsToUserAsync(userId, request.TransferToAccountId, cancellationToken))
        {
            return MutationResult<TransactionDto>.Fail("Destination account not found.");
        }

        // Modeled as one row (see Domain.Entities.Transaction), so this single
        // SaveChangesAsync call is already the atomic unit the spec requires for
        // transfers — there is no second side to roll back independently.
        var transaction = new Transaction
        {
            UserId = userId,
            TransactionDate = request.TransactionDate,
            TransactionType = TransactionType.Transfer,
            AccountId = request.AccountId,
            TransferToAccountId = request.TransferToAccountId,
            Amount = request.Amount,
            Description = request.Description,
            Notes = request.Notes,
        };

        await AttachTagsAsync(userId, transaction, request.Tags, cancellationToken);
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<TransactionDto>.Ok((await queries.GetByIdAsync(userId, transaction.Id, cancellationToken))!);
    }

    public async Task<MutationResult<TransactionDto>> UpdateAsync(Guid userId, Guid transactionId, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await db.Transactions
            .Include(t => t.TransactionTags)
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId, cancellationToken);

        if (transaction is null)
        {
            return MutationResult<TransactionDto>.NotFound();
        }

        if (!await AccountBelongsToUserAsync(userId, request.AccountId, cancellationToken))
        {
            return MutationResult<TransactionDto>.Fail("Account not found.");
        }

        switch (transaction.TransactionType)
        {
            case TransactionType.Expense:
                if (request.ExpenseCategoryId is null || request.IncomeCategoryId is not null
                    || request.IncomeSubCategoryId is not null || request.TransferToAccountId is not null)
                {
                    return MutationResult<TransactionDto>.Fail("An expense must have an expense category and no income or transfer fields.");
                }

                if (!await ExpenseCategoryValidAsync(userId, request.ExpenseCategoryId.Value, request.ExpenseSubCategoryId, cancellationToken))
                {
                    return MutationResult<TransactionDto>.Fail("Category not found.");
                }

                break;

            case TransactionType.Income:
                if (request.IncomeCategoryId is null || request.ExpenseCategoryId is not null
                    || request.ExpenseSubCategoryId is not null || request.TransferToAccountId is not null)
                {
                    return MutationResult<TransactionDto>.Fail("Income must have an income category and no expense or transfer fields.");
                }

                if (!await IncomeCategoryValidAsync(userId, request.IncomeCategoryId.Value, request.IncomeSubCategoryId, cancellationToken))
                {
                    return MutationResult<TransactionDto>.Fail("Category not found.");
                }

                break;

            case TransactionType.Transfer:
                if (request.TransferToAccountId is null || request.ExpenseCategoryId is not null || request.IncomeCategoryId is not null)
                {
                    return MutationResult<TransactionDto>.Fail("A transfer must have a destination account and no category fields.");
                }

                if (request.TransferToAccountId == request.AccountId)
                {
                    return MutationResult<TransactionDto>.Fail("Cannot transfer an account to itself.");
                }

                if (!await AccountBelongsToUserAsync(userId, request.TransferToAccountId.Value, cancellationToken))
                {
                    return MutationResult<TransactionDto>.Fail("Destination account not found.");
                }

                break;

            case TransactionType.Adjustment:
                if (request.ExpenseCategoryId is not null || request.IncomeCategoryId is not null || request.TransferToAccountId is not null)
                {
                    return MutationResult<TransactionDto>.Fail("An adjustment must not have category or transfer fields.");
                }

                break;
        }

        transaction.TransactionDate = request.TransactionDate;
        transaction.AccountId = request.AccountId;
        transaction.Amount = request.Amount;
        transaction.TransferToAccountId = request.TransferToAccountId;
        transaction.ExpenseCategoryId = request.ExpenseCategoryId;
        transaction.ExpenseSubCategoryId = request.ExpenseSubCategoryId;
        transaction.IncomeCategoryId = request.IncomeCategoryId;
        transaction.IncomeSubCategoryId = request.IncomeSubCategoryId;
        transaction.Description = request.Description;
        transaction.Notes = request.Notes;
        transaction.Payee = request.Payee;
        transaction.ReferenceNumber = request.ReferenceNumber;
        transaction.UpdatedAt = DateTime.UtcNow;

        // PUT semantics: tags are fully replaced, not merged.
        db.TransactionTags.RemoveRange(transaction.TransactionTags);
        transaction.TransactionTags.Clear();
        await AttachTagsAsync(userId, transaction, request.Tags, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return MutationResult<TransactionDto>.Ok((await queries.GetByIdAsync(userId, transaction.Id, cancellationToken))!);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId, cancellationToken);
        if (transaction is null)
        {
            return false;
        }

        transaction.IsDeleted = true;
        transaction.DeletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

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

    private async Task AttachTagsAsync(Guid userId, Transaction transaction, IReadOnlyList<string>? tagNames, CancellationToken cancellationToken)
    {
        var tags = await ResolveTagsAsync(userId, tagNames, cancellationToken);
        foreach (var tag in tags)
        {
            db.TransactionTags.Add(new TransactionTag { TransactionId = transaction.Id, TagId = tag.Id });
        }
    }

    private async Task<List<Tag>> ResolveTagsAsync(Guid userId, IReadOnlyList<string>? tagNames, CancellationToken cancellationToken)
    {
        if (tagNames is null || tagNames.Count == 0)
        {
            return [];
        }

        var distinctNames = tagNames
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Distinct()
            .ToList();

        if (distinctNames.Count == 0)
        {
            return [];
        }

        var existingTags = await db.Tags
            .Where(t => t.UserId == userId && distinctNames.Contains(t.Name))
            .ToListAsync(cancellationToken);

        var existingNames = existingTags.Select(t => t.Name).ToHashSet();
        var tags = new List<Tag>(existingTags);

        foreach (var name in distinctNames.Where(n => !existingNames.Contains(n)))
        {
            var tag = new Tag { UserId = userId, Name = name };
            db.Tags.Add(tag);
            tags.Add(tag);
        }

        return tags;
    }
}
