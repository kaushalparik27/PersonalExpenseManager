using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Accounts.Dtos;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class AccountQueries(ApplicationDbContext db) : IAccountQueries
{
    public async Task<IReadOnlyList<AccountDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await db.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);

    public async Task<AccountDto?> GetByIdForUserAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default) =>
        await db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == accountId && a.UserId == userId)
            .Select(a => ToDto(a))
            .FirstOrDefaultAsync(cancellationToken);

    private static AccountDto ToDto(Domain.Entities.Account a) => new(
        a.Id,
        a.Name,
        a.AccountType.ToString(),
        a.Description,
        a.OpeningBalance,
        a.Currency,
        a.IsActive,
        a.DisplayOrder,
        a.Icon,
        a.CreatedAt,
        a.UpdatedAt);
}
