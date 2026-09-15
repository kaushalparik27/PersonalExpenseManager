using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Transactions.Dtos;

namespace PersonalExpenseManager.Application.Transactions;

public interface ITransactionCommands
{
    Task<MutationResult<TransactionDto>> CreateExpenseAsync(Guid userId, CreateExpenseRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<TransactionDto>> CreateIncomeAsync(Guid userId, CreateIncomeRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<TransactionDto>> CreateTransferAsync(Guid userId, CreateTransferRequest request, CancellationToken cancellationToken = default);

    Task<MutationResult<TransactionDto>> UpdateAsync(Guid userId, Guid transactionId, UpdateTransactionRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
}
