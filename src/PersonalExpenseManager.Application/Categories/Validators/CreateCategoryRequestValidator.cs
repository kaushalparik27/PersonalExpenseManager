using FluentValidation;
using PersonalExpenseManager.Application.Categories.Dtos;

namespace PersonalExpenseManager.Application.Categories.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}
