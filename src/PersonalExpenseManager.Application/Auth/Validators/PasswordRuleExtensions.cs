using FluentValidation;

namespace PersonalExpenseManager.Application.Auth.Validators;

public static class PasswordRuleExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeAStrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
}
