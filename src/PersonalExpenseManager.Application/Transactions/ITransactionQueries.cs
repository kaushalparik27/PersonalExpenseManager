using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Transactions.Dtos;

namespace PersonalExpenseManager.Application.Transactions;

public interface ITransactionQueries
{
    Task<PagedResult<TransactionDto>> GetPagedAsync(Guid userId, TransactionListQuery query, CancellationToken cancellationToken = default);

    Task<TransactionDto?> GetByIdAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
}
