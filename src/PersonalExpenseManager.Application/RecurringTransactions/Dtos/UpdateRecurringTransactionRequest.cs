namespace PersonalExpenseManager.Application.RecurringTransactions.Dtos;

// Fields relevant to the recurring transaction's own type must be supplied; fields
// that don't apply (e.g. an income category on a transfer) must be left null — the
// command layer rejects any mismatch against the existing TransactionType, which
// itself is never changed by an update (mirrors UpdateTransactionRequest).
public record UpdateRecurringTransactionRequest(
    Guid AccountId,
    decimal Amount,
    Guid? TransferToAccountId,
    Guid? ExpenseCategoryId,
    Guid? ExpenseSubCategoryId,
    Guid? IncomeCategoryId,
    Guid? IncomeSubCategoryId,
    string? Description,
    string Frequency,
    DateOnly StartDate,
    DateOnly? EndDate);
