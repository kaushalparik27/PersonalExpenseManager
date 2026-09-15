using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Api.Filters;

// Centralizes FluentValidation for any action argument that has a registered
// IValidator<T>, so controllers don't hand-roll validation calls per endpoint.
public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => new ApiError(e.PropertyName, e.ErrorMessage));
                context.Result = new UnprocessableEntityObjectResult(ApiResponse<object>.Fail(errors));
                return;
            }
        }

        await next();
    }
}
