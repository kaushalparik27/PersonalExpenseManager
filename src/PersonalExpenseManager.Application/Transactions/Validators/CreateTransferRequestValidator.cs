using FluentValidation;
using PersonalExpenseManager.Application.Transactions.Dtos;

namespace PersonalExpenseManager.Application.Transactions.Validators;

public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.TransferToAccountId).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.AccountId != x.TransferToAccountId)
            .WithMessage("Cannot transfer an account to itself.")
            .WithName(nameof(CreateTransferRequest.TransferToAccountId));
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Tags).MustBeValidTags();
    }
}
