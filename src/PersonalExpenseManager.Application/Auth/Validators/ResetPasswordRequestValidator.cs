using FluentValidation;
using PersonalExpenseManager.Application.Auth.Dtos;

namespace PersonalExpenseManager.Application.Auth.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).MustBeAStrongPassword();
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}
