using PersonalExpenseManager.Application.Accounts.Dtos;

namespace PersonalExpenseManager.Application.Accounts;

// Minimal read-side query surface added during the Authentication phase solely to
// prove and test per-user ownership isolation. The Accounts phase extends this
// interface with create/update/delete and balance-calculation logic; it does not
// replace it.
public interface IAccountQueries
{
    Task<IReadOnlyList<AccountDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AccountDto?> GetByIdForUserAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
}
