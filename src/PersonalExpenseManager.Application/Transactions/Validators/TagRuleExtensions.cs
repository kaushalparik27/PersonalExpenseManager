using FluentValidation;
using PersonalExpenseManager.Application.Transactions.Dtos;

namespace PersonalExpenseManager.Application.Transactions.Validators;

public static class TagRuleExtensions
{
    public static IRuleBuilderOptions<T, IReadOnlyList<string>?> MustBeValidTags<T>(this IRuleBuilder<T, IReadOnlyList<string>?> rule) =>
        rule.Must(tags => tags is null || tags.Count <= 10)
            .WithMessage("A transaction can have at most 10 tags.")
            .Must(tags => tags is null || tags.All(t => !string.IsNullOrWhiteSpace(t) && t.Length <= 50))
            .WithMessage("Each tag must be 1-50 characters.");
}
