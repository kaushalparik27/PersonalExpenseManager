using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Transactions;
using PersonalExpenseManager.Application.Transactions.Dtos;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class TransactionQueries(ApplicationDbContext db) : ITransactionQueries
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<TransactionDto>> GetPagedAsync(Guid userId, TransactionListQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var transactionsQuery = db.Transactions.AsNoTracking().Where(t => t.UserId == userId);

        if (query.FromDate is { } fromDate)
        {
            transactionsQuery = transactionsQuery.Where(t => t.TransactionDate >= fromDate);
        }

        if (query.ToDate is { } toDate)
        {
            transactionsQuery = transactionsQuery.Where(t => t.TransactionDate <= toDate);
        }

        if (query.AccountId is { } accountId)
        {
            transactionsQuery = transactionsQuery.Where(t => t.AccountId == accountId || t.TransferToAccountId == accountId);
        }

        if (query.CategoryId is { } categoryId)
        {
            transactionsQuery = transactionsQuery.Where(t => t.ExpenseCategoryId == categoryId || t.IncomeCategoryId == categoryId);
        }

        if (query.SubCategoryId is { } subCategoryId)
        {
            transactionsQuery = transactionsQuery.Where(t => t.ExpenseSubCategoryId == subCategoryId || t.IncomeSubCategoryId == subCategoryId);
        }

        if (query.TransactionType is { Length: > 0 } typeString && Enum.TryParse<TransactionType>(typeString, ignoreCase: true, out var type))
        {
            transactionsQuery = transactionsQuery.Where(t => t.TransactionType == type);
        }

        if (query.MinAmount is { } minAmount)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Amount >= minAmount);
        }

        if (query.MaxAmount is { } maxAmount)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Amount <= maxAmount);
        }

        if (query.Search is { Length: > 0 } search)
        {
            var pattern = $"%{search}%";

            var matchingAccountIds = await db.Accounts
                .Where(a => a.UserId == userId && EF.Functions.ILike(a.Name, pattern))
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            var matchingExpenseCategoryIds = await db.ExpenseCategories
                .Where(c => c.UserId == userId && EF.Functions.ILike(c.Name, pattern))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            var matchingIncomeCategoryIds = await db.IncomeCategories
                .Where(c => c.UserId == userId && EF.Functions.ILike(c.Name, pattern))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            var matchingTaggedTransactionIds = await db.TransactionTags
                .Where(tt => tt.Tag!.UserId == userId && EF.Functions.ILike(tt.Tag.Name, pattern))
                .Select(tt => tt.TransactionId)
                .ToListAsync(cancellationToken);

            transactionsQuery = transactionsQuery.Where(t =>
                EF.Functions.ILike(t.Description ?? "", pattern) ||
                EF.Functions.ILike(t.Notes ?? "", pattern) ||
                EF.Functions.ILike(t.Payee ?? "", pattern) ||
                EF.Functions.ILike(t.ReferenceNumber ?? "", pattern) ||
                matchingAccountIds.Contains(t.AccountId) ||
                (t.TransferToAccountId != null && matchingAccountIds.Contains(t.TransferToAccountId.Value)) ||
                (t.ExpenseCategoryId != null && matchingExpenseCategoryIds.Contains(t.ExpenseCategoryId.Value)) ||
                (t.IncomeCategoryId != null && matchingIncomeCategoryIds.Contains(t.IncomeCategoryId.Value)) ||
                matchingTaggedTransactionIds.Contains(t.Id));
        }

        var totalCount = await transactionsQuery.CountAsync(cancellationToken);

        transactionsQuery = ApplySort(transactionsQuery, query.SortBy, query.SortDirection);

        var pageOfTransactions = await transactionsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = await MapToDtosAsync(pageOfTransactions, cancellationToken);

        return new PagedResult<TransactionDto>(dtos, page, pageSize, totalCount);
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await db.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == userId, cancellationToken);

        if (transaction is null)
        {
            return null;
        }

        var dtos = await MapToDtosAsync([transaction], cancellationToken);
        return dtos[0];
    }

    private static IQueryable<Transaction> ApplySort(IQueryable<Transaction> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "amount" => descending ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
            "createdat" => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            "description" => descending ? query.OrderByDescending(t => t.Description) : query.OrderBy(t => t.Description),
            _ => descending
                ? query.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.TransactionDate).ThenBy(t => t.CreatedAt),
        };
    }

    private async Task<List<TransactionDto>> MapToDtosAsync(IReadOnlyList<Transaction> transactions, CancellationToken cancellationToken)
    {
        var accountIds = transactions
            .SelectMany(t => new[] { t.AccountId, t.TransferToAccountId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var expenseCategoryIds = transactions.Where(t => t.ExpenseCategoryId.HasValue).Select(t => t.ExpenseCategoryId!.Value).Distinct().ToList();
        var expenseSubCategoryIds = transactions.Where(t => t.ExpenseSubCategoryId.HasValue).Select(t => t.ExpenseSubCategoryId!.Value).Distinct().ToList();
        var incomeCategoryIds = transactions.Where(t => t.IncomeCategoryId.HasValue).Select(t => t.IncomeCategoryId!.Value).Distinct().ToList();
        var incomeSubCategoryIds = transactions.Where(t => t.IncomeSubCategoryId.HasValue).Select(t => t.IncomeSubCategoryId!.Value).Distinct().ToList();
        var transactionIds = transactions.Select(t => t.Id).ToList();

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

        var tagsByTransactionId = await db.TransactionTags.AsNoTracking()
            .Where(tt => transactionIds.Contains(tt.TransactionId))
            .Select(tt => new { tt.TransactionId, TagName = tt.Tag!.Name })
            .ToListAsync(cancellationToken);

        var tagsLookup = tagsByTransactionId
            .GroupBy(x => x.TransactionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.TagName).ToList());

        return transactions.Select(t => new TransactionDto(
            t.Id,
            t.TransactionDate,
            t.TransactionType.ToString(),
            t.AccountId,
            accountNames.GetValueOrDefault(t.AccountId, "Unknown Account"),
            t.TransferToAccountId,
            t.TransferToAccountId.HasValue ? accountNames.GetValueOrDefault(t.TransferToAccountId.Value) : null,
            t.Amount,
            t.ExpenseCategoryId,
            t.ExpenseCategoryId.HasValue ? expenseCategoryNames.GetValueOrDefault(t.ExpenseCategoryId.Value) : null,
            t.ExpenseSubCategoryId,
            t.ExpenseSubCategoryId.HasValue ? expenseSubCategoryNames.GetValueOrDefault(t.ExpenseSubCategoryId.Value) : null,
            t.IncomeCategoryId,
            t.IncomeCategoryId.HasValue ? incomeCategoryNames.GetValueOrDefault(t.IncomeCategoryId.Value) : null,
            t.IncomeSubCategoryId,
            t.IncomeSubCategoryId.HasValue ? incomeSubCategoryNames.GetValueOrDefault(t.IncomeSubCategoryId.Value) : null,
            t.Description,
            t.Notes,
            t.Payee,
            t.ReferenceNumber,
            tagsLookup.GetValueOrDefault(t.Id, []),
            t.CreatedAt,
            t.UpdatedAt))
            .ToList();
    }
}
