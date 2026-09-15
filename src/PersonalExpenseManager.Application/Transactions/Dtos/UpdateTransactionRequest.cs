namespace PersonalExpenseManager.Application.Transactions.Dtos;

// Fields relevant to the transaction's own type must be supplied; fields that
// don't apply (e.g. an income category on a transfer) must be left null — the
// command layer rejects any mismatch against the transaction's existing
// TransactionType, which itself is never changed by an update.
public record UpdateTransactionRequest(
    DateOnly TransactionDate,
    Guid AccountId,
    decimal Amount,
    Guid? TransferToAccountId,
    Guid? ExpenseCategoryId,
    Guid? ExpenseSubCategoryId,
    Guid? IncomeCategoryId,
    Guid? IncomeSubCategoryId,
    string? Description,
    string? Notes,
    string? Payee,
    string? ReferenceNumber,
    IReadOnlyList<string>? Tags);
