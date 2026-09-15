using FluentValidation;
using PersonalExpenseManager.Application.Accounts.Dtos;

namespace PersonalExpenseManager.Application.Accounts.Validators;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountType).IsInEnum();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Currency).NotEmpty().Matches("^[A-Z]{3}$").WithMessage("Currency must be a 3-letter ISO code, e.g. INR.");
        RuleFor(x => x.Icon).MaximumLength(50);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
