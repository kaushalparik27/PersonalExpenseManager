namespace PersonalExpenseManager.Application.Auth.Dtos;

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword);
