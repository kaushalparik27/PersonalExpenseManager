namespace PersonalExpenseManager.Application.RecurringTransactions.Dtos;

public record CreateRecurringIncomeRequest(
    Guid AccountId,
    decimal Amount,
    Guid IncomeCategoryId,
    Guid? IncomeSubCategoryId,
    string? Description,
    string Frequency,
    DateOnly StartDate,
    DateOnly? EndDate);
