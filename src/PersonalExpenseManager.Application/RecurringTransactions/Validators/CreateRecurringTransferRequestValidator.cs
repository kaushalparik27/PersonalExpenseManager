using FluentValidation;
using PersonalExpenseManager.Application.RecurringTransactions.Dtos;

namespace PersonalExpenseManager.Application.RecurringTransactions.Validators;

public class CreateRecurringTransferRequestValidator : AbstractValidator<CreateRecurringTransferRequest>
{
    public CreateRecurringTransferRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.TransferToAccountId).NotEmpty();
        RuleFor(x => x.TransferToAccountId).NotEqual(x => x.AccountId).WithMessage("Cannot transfer an account to itself.");
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Frequency).Must(RecurrenceRuleExtensions.BeAValidFrequency).WithMessage("Frequency must be one of Daily, Weekly, Monthly, Quarterly, Yearly.");
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate is not null).WithMessage("End date must be on or after the start date.");
    }
}
