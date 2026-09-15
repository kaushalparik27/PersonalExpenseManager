namespace PersonalExpenseManager.Application.RecurringTransactions.Dtos;

public record RecurringTransactionDto(
    Guid Id,
    string TransactionType,
    Guid AccountId,
    string AccountName,
    Guid? TransferToAccountId,
    string? TransferToAccountName,
    decimal Amount,
    Guid? ExpenseCategoryId,
    string? ExpenseCategoryName,
    Guid? ExpenseSubCategoryId,
    string? ExpenseSubCategoryName,
    Guid? IncomeCategoryId,
    string? IncomeCategoryName,
    Guid? IncomeSubCategoryId,
    string? IncomeSubCategoryName,
    string? Description,
    string Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly NextOccurrenceDate,
    bool IsActive);
