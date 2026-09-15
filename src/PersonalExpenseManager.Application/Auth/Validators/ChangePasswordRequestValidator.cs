using FluentValidation;
using PersonalExpenseManager.Application.Auth.Dtos;

namespace PersonalExpenseManager.Application.Auth.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).MustBeAStrongPassword();
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}
