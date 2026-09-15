using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Application.Accounts.Dtos;

public record CreateAccountRequest(
    string Name,
    AccountType AccountType,
    string? Description,
    decimal OpeningBalance,
    string Currency,
    string? Icon,
    int DisplayOrder);
