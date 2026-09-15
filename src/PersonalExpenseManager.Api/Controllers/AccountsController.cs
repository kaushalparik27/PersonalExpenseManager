using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(IAccountQueries accountQueries, IAccountCommands accountCommands) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var accounts = await accountQueries.GetForUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AccountDto>>.Ok(accounts));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var account = await accountQueries.GetByIdForUserAsync(userId, id, cancellationToken);

        if (account is null)
        {
            return NotFound(ApiResponse<object>.Fail("Account not found."));
        }

        return Ok(ApiResponse<AccountDto>.Ok(account));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var account = await accountCommands.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, ApiResponse<AccountDto>.Ok(account));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var account = await accountCommands.UpdateAsync(userId, id, request, cancellationToken);

        if (account is null)
        {
            return NotFound(ApiResponse<object>.Fail("Account not found."));
        }

        return Ok(ApiResponse<AccountDto>.Ok(account));
    }

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var archived = await accountCommands.ArchiveAsync(userId, id, cancellationToken);

        if (!archived)
        {
            return NotFound(ApiResponse<object>.Fail("Account not found."));
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await accountCommands.DeleteAsync(userId, id, cancellationToken);

        return result switch
        {
            AccountDeleteResult.NotFound => NotFound(ApiResponse<object>.Fail("Account not found.")),
            AccountDeleteResult.HasTransactions => Conflict(
                ApiResponse<object>.Fail("This account has existing transactions. Archive it instead of deleting.")),
            AccountDeleteResult.Deleted => NoContent(),
            _ => throw new InvalidOperationException($"Unhandled {nameof(AccountDeleteResult)} value: {result}"),
        };
    }
}
