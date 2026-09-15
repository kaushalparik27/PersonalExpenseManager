using FluentValidation;
using PersonalExpenseManager.Application.Categories.Dtos;

namespace PersonalExpenseManager.Application.Categories.Validators;

public class UpdateSubCategoryRequestValidator : AbstractValidator<UpdateSubCategoryRequest>
{
    public UpdateSubCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}
