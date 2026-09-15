namespace PersonalExpenseManager.Application.RecurringTransactions.Dtos;

public record CreateRecurringExpenseRequest(
    Guid AccountId,
    decimal Amount,
    Guid ExpenseCategoryId,
    Guid? ExpenseSubCategoryId,
    string? Description,
    string Frequency,
    DateOnly StartDate,
    DateOnly? EndDate);
