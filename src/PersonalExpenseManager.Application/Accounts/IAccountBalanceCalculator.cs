namespace PersonalExpenseManager.Application.Accounts;

// The single source of truth for Account.CurrentBalance:
// OpeningBalance + Income - Expense + TransferIn - TransferOut + Adjustments.
// Every screen and report must go through this rather than computing its own
// version of the formula.
public interface IAccountBalanceCalculator
{
    Task<IReadOnlyDictionary<Guid, decimal>> CalculateCurrentBalancesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<decimal> CalculateCurrentBalanceAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
}
