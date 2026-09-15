using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.RecurringTransactions.Dtos;

namespace PersonalExpenseManager.Application.RecurringTransactions;

public interface IRecurringTransactionCommands
{
    Task<MutationResult<RecurringTransactionDto>> CreateExpenseAsync(Guid userId, CreateRecurringExpenseRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<RecurringTransactionDto>> CreateIncomeAsync(Guid userId, CreateRecurringIncomeRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<RecurringTransactionDto>> CreateTransferAsync(Guid userId, CreateRecurringTransferRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<RecurringTransactionDto>> UpdateAsync(Guid userId, Guid recurringTransactionId, UpdateRecurringTransactionRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<RecurringTransactionDto>> SetActiveAsync(Guid userId, Guid recurringTransactionId, bool isActive, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid userId, Guid recurringTransactionId, CancellationToken cancellationToken = default);
}
