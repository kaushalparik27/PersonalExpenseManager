namespace PersonalExpenseManager.Application.Transactions.Dtos;

public record CreateIncomeRequest(
    DateOnly TransactionDate,
    Guid AccountId,
    decimal Amount,
    Guid IncomeCategoryId,
    Guid? IncomeSubCategoryId,
    string? Description,
    string? Notes,
    string? Payee,
    string? ReferenceNumber,
    IReadOnlyList<string>? Tags);
