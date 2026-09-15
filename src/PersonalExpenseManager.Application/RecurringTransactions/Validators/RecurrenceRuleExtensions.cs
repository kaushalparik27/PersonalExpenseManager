using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Application.RecurringTransactions.Validators;

public static class RecurrenceRuleExtensions
{
    public static bool BeAValidFrequency(string frequency) =>
        Enum.TryParse<RecurrenceFrequency>(frequency, ignoreCase: true, out _);
}
