using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class AccountBalanceCalculator(ApplicationDbContext db) : IAccountBalanceCalculator
{
    private record AccountTypeTotal(Guid AccountId, TransactionType TransactionType, decimal Total);

    private record TypeTotal(TransactionType TransactionType, decimal Total);

    public async Task<IReadOnlyDictionary<Guid, decimal>> CalculateCurrentBalancesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new { a.Id, a.OpeningBalance })
            .ToListAsync(cancellationToken);

        if (accounts.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var accountIds = accounts.Select(a => a.Id).ToList();

        var byAccountAndType = await db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && accountIds.Contains(t.AccountId))
            .GroupBy(t => new { t.AccountId, t.TransactionType })
            .Select(g => new AccountTypeTotal(g.Key.AccountId, g.Key.TransactionType, g.Sum(t => t.Amount)))
            .ToListAsync(cancellationToken);

        var transfersIn = await db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId
                && t.TransactionType == TransactionType.Transfer
                && t.TransferToAccountId != null
                && accountIds.Contains(t.TransferToAccountId!.Value))
            .GroupBy(t => t.TransferToAccountId!.Value)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        decimal SumFor(Guid accountId, TransactionType type) =>
            byAccountAndType.Where(r => r.AccountId == accountId && r.TransactionType == type).Sum(r => r.Total);

        return accounts.ToDictionary(
            a => a.Id,
            a => ComputeBalance(
                a.OpeningBalance,
                income: SumFor(a.Id, TransactionType.Income),
                expense: SumFor(a.Id, TransactionType.Expense),
                transferIn: transfersIn.Where(r => r.AccountId == a.Id).Sum(r => r.Total),
                transferOut: SumFor(a.Id, TransactionType.Transfer),
                adjustments: SumFor(a.Id, TransactionType.Adjustment)));
    }

    public async Task<decimal> CalculateCurrentBalanceAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var openingBalance = await db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == accountId && a.UserId == userId)
            .Select(a => (decimal?)a.OpeningBalance)
            .FirstOrDefaultAsync(cancellationToken);

        if (openingBalance is null)
        {
            return 0m;
        }

        var byType = await db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.AccountId == accountId)
            .GroupBy(t => t.TransactionType)
            .Select(g => new TypeTotal(g.Key, g.Sum(t => t.Amount)))
            .ToListAsync(cancellationToken);

        var transferIn = await db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Transfer && t.TransferToAccountId == accountId)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        decimal SumFor(TransactionType type) => byType.Where(r => r.TransactionType == type).Sum(r => r.Total);

        return ComputeBalance(
            openingBalance.Value,
            income: SumFor(TransactionType.Income),
            expense: SumFor(TransactionType.Expense),
            transferIn: transferIn,
            transferOut: SumFor(TransactionType.Transfer),
            adjustments: SumFor(TransactionType.Adjustment));
    }

    private static decimal ComputeBalance(
        decimal openingBalance,
        decimal income,
        decimal expense,
        decimal transferIn,
        decimal transferOut,
        decimal adjustments) =>
        openingBalance + income - expense + transferIn - transferOut + adjustments;
}
