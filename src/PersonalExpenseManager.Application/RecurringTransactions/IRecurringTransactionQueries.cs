using PersonalExpenseManager.Application.RecurringTransactions.Dtos;

namespace PersonalExpenseManager.Application.RecurringTransactions;

public interface IRecurringTransactionQueries
{
    Task<IReadOnlyList<RecurringTransactionDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<RecurringTransactionDto?> GetByIdAsync(Guid userId, Guid recurringTransactionId, CancellationToken cancellationToken = default);
}
