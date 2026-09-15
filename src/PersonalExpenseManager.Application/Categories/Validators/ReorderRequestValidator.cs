using FluentValidation;
using PersonalExpenseManager.Application.Categories.Dtos;

namespace PersonalExpenseManager.Application.Categories.Validators;

public class ReorderRequestValidator : AbstractValidator<ReorderRequest>
{
    public ReorderRequestValidator()
    {
        RuleFor(x => x.OrderedIds).NotEmpty();
    }
}
