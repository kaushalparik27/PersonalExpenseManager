namespace PersonalExpenseManager.Application.Transactions.Dtos;

public record CreateTransferRequest(
    DateOnly TransactionDate,
    Guid AccountId,
    Guid TransferToAccountId,
    decimal Amount,
    string? Description,
    string? Notes,
    IReadOnlyList<string>? Tags);
