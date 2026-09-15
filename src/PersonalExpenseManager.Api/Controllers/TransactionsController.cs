using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Transactions;
using PersonalExpenseManager.Application.Transactions.Dtos;

namespace PersonalExpenseManager.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(ITransactionQueries queries, ITransactionCommands commands) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TransactionListQuery query, CancellationToken cancellationToken)
    {
        var result = await queries.GetPagedAsync(User.GetUserId(), query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TransactionDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await queries.GetByIdAsync(User.GetUserId(), id, cancellationToken);
        if (transaction is null)
        {
            return NotFound(ApiResponse<object>.Fail("Transaction not found."));
        }

        return Ok(ApiResponse<TransactionDto>.Ok(transaction));
    }

    [HttpPost("expense")]
    public async Task<IActionResult> CreateExpense(CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateExpenseAsync(User.GetUserId(), request, cancellationToken);
        return RespondToMutation(result, nameof(GetById));
    }

    [HttpPost("income")]
    public async Task<IActionResult> CreateIncome(CreateIncomeRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateIncomeAsync(User.GetUserId(), request, cancellationToken);
        return RespondToMutation(result, nameof(GetById));
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer(CreateTransferRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateTransferAsync(User.GetUserId(), request, cancellationToken);
        return RespondToMutation(result, nameof(GetById));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.UpdateAsync(User.GetUserId(), id, request, cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Transaction not found."));
        }

        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return Ok(ApiResponse<TransactionDto>.Ok(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await commands.DeleteAsync(User.GetUserId(), id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Transaction not found."));
        }

        return NoContent();
    }

    private IActionResult RespondToMutation(MutationResult<TransactionDto> result, string getActionName)
    {
        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return CreatedAtAction(getActionName, new { id = result.Value!.Id }, ApiResponse<TransactionDto>.Ok(result.Value));
    }
}
