namespace PersonalExpenseManager.Application.Accounts.Dtos;

public record AccountDto(
    Guid Id,
    string Name,
    string AccountType,
    string? Description,
    decimal OpeningBalance,
    string Currency,
    bool IsActive,
    int DisplayOrder,
    string? Icon,
    DateTime CreatedAt,
    DateTime UpdatedAt);
