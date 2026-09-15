namespace PersonalExpenseManager.Application.Transactions.Dtos;

public record CreateExpenseRequest(
    DateOnly TransactionDate,
    Guid AccountId,
    decimal Amount,
    Guid ExpenseCategoryId,
    Guid? ExpenseSubCategoryId,
    string? Description,
    string? Notes,
    string? Payee,
    string? ReferenceNumber,
    IReadOnlyList<string>? Tags);
