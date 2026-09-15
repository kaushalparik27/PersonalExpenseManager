namespace PersonalExpenseManager.Application.RecurringTransactions.Dtos;

public record CreateRecurringTransferRequest(
    Guid AccountId,
    Guid TransferToAccountId,
    decimal Amount,
    string? Description,
    string Frequency,
    DateOnly StartDate,
    DateOnly? EndDate);
