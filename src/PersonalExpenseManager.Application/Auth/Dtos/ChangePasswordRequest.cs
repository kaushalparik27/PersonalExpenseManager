namespace PersonalExpenseManager.Application.Auth.Dtos;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
