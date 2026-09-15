using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Domain.Entities;

namespace PersonalExpenseManager.Infrastructure.Persistence.Commands;

public class AccountCommands(ApplicationDbContext db, IAccountBalanceCalculator balanceCalculator) : IAccountCommands
{
    public async Task<AccountDto> CreateAsync(Guid userId, CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            UserId = userId,
            Name = request.Name,
            AccountType = request.AccountType,
            Description = request.Description,
            OpeningBalance = request.OpeningBalance,
            Currency = request.Currency,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(account, account.OpeningBalance);
    }

    public async Task<AccountDto?> UpdateAsync(Guid userId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);
        if (account is null)
        {
            return null;
        }

        account.Name = request.Name;
        account.AccountType = request.AccountType;
        account.Description = request.Description;
        account.OpeningBalance = request.OpeningBalance;
        account.Currency = request.Currency;
        account.Icon = request.Icon;
        account.DisplayOrder = request.DisplayOrder;
        account.IsActive = request.IsActive;
        account.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        var balance = await balanceCalculator.CalculateCurrentBalanceAsync(userId, accountId, cancellationToken);
        return ToDto(account, balance);
    }

    public async Task<bool> ArchiveAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);
        if (account is null)
        {
            return false;
        }

        account.IsActive = false;
        account.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UnhideAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);
        if (account is null)
        {
            return false;
        }

        account.IsActive = true;
        account.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AccountDeleteResult> DeleteAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);
        if (account is null)
        {
            return AccountDeleteResult.NotFound;
        }

        // Soft-deleted transactions still count: they're recoverable, so the
        // account they reference must not be removed out from under them.
        var hasTransactions = await db.Transactions
            .IgnoreQueryFilters()
            .AnyAsync(t => t.AccountId == accountId || t.TransferToAccountId == accountId, cancellationToken);

        if (hasTransactions)
        {
            return AccountDeleteResult.HasTransactions;
        }

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(cancellationToken);

        return AccountDeleteResult.Deleted;
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
