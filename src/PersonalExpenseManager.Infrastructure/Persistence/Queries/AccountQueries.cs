using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Domain.Entities;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class AccountQueries(ApplicationDbContext db, IAccountBalanceCalculator balanceCalculator) : IAccountQueries
{
    public async Task<IReadOnlyList<AccountDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(cancellationToken);

        var balances = await balanceCalculator.CalculateCurrentBalancesAsync(userId, cancellationToken);

        return accounts
            .Select(a => ToDto(a, balances.GetValueOrDefault(a.Id, a.OpeningBalance)))
            .ToList();
    }

    public async Task<AccountDto?> GetByIdForUserAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);

        if (account is null)
        {
            return null;
        }

        var balance = await balanceCalculator.CalculateCurrentBalanceAsync(userId, accountId, cancellationToken);
        return ToDto(account, balance);
    }

    private static AccountDto ToDto(Account a, decimal currentBalance) => new(
        a.Id,
        a.Name,
        a.AccountType.ToString(),
        a.Description,
        a.OpeningBalance,
        currentBalance,
        a.Currency,
        a.IsActive,
        a.DisplayOrder,
        a.Icon,
        a.CreatedAt,
        a.UpdatedAt);
}
