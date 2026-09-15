namespace PersonalExpenseManager.Application.Accounts.Dtos;

public record AccountDto(
    Guid Id,
    string Name,
    string AccountType,
    string? Description,
    decimal OpeningBalance,
    decimal CurrentBalance,
    string Currency,
    bool IsActive,
    int DisplayOrder,
    string? Icon,
    DateTime CreatedAt,
    DateTime UpdatedAt);
