namespace PersonalExpenseManager.Application.Auth.Dtos;

public record UserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);
