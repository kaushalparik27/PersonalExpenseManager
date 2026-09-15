using PersonalExpenseManager.Application.Accounts.Dtos;

namespace PersonalExpenseManager.Application.Accounts;

public enum AccountDeleteResult
{
    NotFound,
    HasTransactions,
    Deleted,
}

public interface IAccountCommands
{
    Task<AccountDto> CreateAsync(Guid userId, CreateAccountRequest request, CancellationToken cancellationToken = default);

    Task<AccountDto?> UpdateAsync(Guid userId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);

    Task<bool> UnhideAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);

    Task<AccountDeleteResult> DeleteAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
}
