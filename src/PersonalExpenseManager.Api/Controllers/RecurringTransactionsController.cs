using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.RecurringTransactions;
using PersonalExpenseManager.Application.RecurringTransactions.Dtos;

namespace PersonalExpenseManager.Api.Controllers;

[ApiController]
[Route("api/recurring")]
public class RecurringTransactionsController(IRecurringTransactionQueries queries, IRecurringTransactionCommands commands) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var recurring = await queries.GetForUserAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RecurringTransactionDto>>.Ok(recurring));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var recurring = await queries.GetByIdAsync(User.GetUserId(), id, cancellationToken);
        if (recurring is null)
        {
            return NotFound(ApiResponse<object>.Fail("Recurring transaction not found."));
        }

        return Ok(ApiResponse<RecurringTransactionDto>.Ok(recurring));
    }

    [HttpPost("expense")]
    public async Task<IActionResult> CreateExpense(CreateRecurringExpenseRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateExpenseAsync(User.GetUserId(), request, cancellationToken);
        return RespondToMutation(result);
    }

    [HttpPost("income")]
    public async Task<IActionResult> CreateIncome(CreateRecurringIncomeRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateIncomeAsync(User.GetUserId(), request, cancellationToken);
        return RespondToMutation(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer(CreateRecurringTransferRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.CreateTransferAsync(User.GetUserId(), request, cancellationToken);
        return RespondToMutation(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRecurringTransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await commands.UpdateAsync(User.GetUserId(), id, request, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Recurring transaction not found."));
        }

        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return Ok(ApiResponse<RecurringTransactionDto>.Ok(result.Value!));
    }

    [HttpPatch("{id:guid}/pause")]
    public async Task<IActionResult> Pause(Guid id, CancellationToken cancellationToken)
    {
        var result = await commands.SetActiveAsync(User.GetUserId(), id, isActive: false, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Recurring transaction not found."));
        }

        return Ok(ApiResponse<RecurringTransactionDto>.Ok(result.Value!));
    }

    [HttpPatch("{id:guid}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken cancellationToken)
    {
        var result = await commands.SetActiveAsync(User.GetUserId(), id, isActive: true, cancellationToken);
        if (result.IsNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Recurring transaction not found."));
        }

        return Ok(ApiResponse<RecurringTransactionDto>.Ok(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await commands.DeleteAsync(User.GetUserId(), id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Recurring transaction not found."));
        }

        return NoContent();
    }

    private IActionResult RespondToMutation(MutationResult<RecurringTransactionDto> result)
    {
        if (result.Error is not null)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ApiResponse<RecurringTransactionDto>.Ok(result.Value));
    }
}
