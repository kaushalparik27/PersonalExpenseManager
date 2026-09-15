using FluentValidation;
using PersonalExpenseManager.Application.Auth.Dtos;

namespace PersonalExpenseManager.Application.Auth.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
