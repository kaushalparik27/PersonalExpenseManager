using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalExpenseManager.Application.RecurringTransactions;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;
using PersonalExpenseManager.Infrastructure.Persistence;

namespace PersonalExpenseManager.Infrastructure.Services;

// The recurring transaction engine (spec §31: "must prevent duplicate transaction
// creation"). NextOccurrenceDate is the single source of truth for what has already
// been generated: it only advances in the same SaveChangesAsync call that inserts
// the Transaction for that date, so a run that is interrupted mid-catch-up can never
// re-generate an occurrence that already committed, and re-running is always safe.
public class RecurringTransactionProcessor(ApplicationDbContext db, ILogger<RecurringTransactionProcessor> logger) : IRecurringTransactionProcessor
{
    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var due = await db.RecurringTransactions
            .Where(r => r.IsActive && r.NextOccurrenceDate <= today)
            .ToListAsync(cancellationToken);

        var generatedCount = 0;

        foreach (var recurring in due)
        {
            generatedCount += await ProcessOneAsync(recurring, today, cancellationToken);
        }

        if (generatedCount > 0)
        {
            logger.LogInformation("Recurring transaction engine generated {Count} transaction(s) across {RecurringCount} template(s).", generatedCount, due.Count);
        }

        return generatedCount;
    }

    private async Task<int> ProcessOneAsync(RecurringTransaction recurring, DateOnly today, CancellationToken cancellationToken)
    {
        var generated = 0;

        while (recurring.NextOccurrenceDate <= today
            && (recurring.EndDate is null || recurring.NextOccurrenceDate <= recurring.EndDate))
        {
            var occurrenceDate = recurring.NextOccurrenceDate;

            db.Transactions.Add(new Transaction
            {
                UserId = recurring.UserId,
                TransactionDate = occurrenceDate,
                TransactionType = recurring.TransactionType,
                AccountId = recurring.AccountId,
                TransferToAccountId = recurring.TransferToAccountId,
                Amount = recurring.Amount,
                ExpenseCategoryId = recurring.ExpenseCategoryId,
                ExpenseSubCategoryId = recurring.ExpenseSubCategoryId,
                IncomeCategoryId = recurring.IncomeCategoryId,
                IncomeSubCategoryId = recurring.IncomeSubCategoryId,
                Description = recurring.Description,
                IsRecurring = true,
                RecurringTransactionId = recurring.Id,
            });

            recurring.NextOccurrenceDate = Advance(occurrenceDate, recurring.Frequency);
            generated++;

            // Commit each occurrence (and the advanced cursor) together, rather than
            // batching the whole catch-up run — see the class-level duplicate-prevention note.
            await db.SaveChangesAsync(cancellationToken);
        }

        if (recurring.EndDate is not null && recurring.NextOccurrenceDate > recurring.EndDate)
        {
            recurring.IsActive = false;
            await db.SaveChangesAsync(cancellationToken);
        }

        return generated;
    }

    private static DateOnly Advance(DateOnly date, RecurrenceFrequency frequency) => frequency switch
    {
        RecurrenceFrequency.Daily => date.AddDays(1),
        RecurrenceFrequency.Weekly => date.AddDays(7),
        RecurrenceFrequency.Monthly => date.AddMonths(1),
        RecurrenceFrequency.Quarterly => date.AddMonths(3),
        RecurrenceFrequency.Yearly => date.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Unknown recurrence frequency."),
    };
}
