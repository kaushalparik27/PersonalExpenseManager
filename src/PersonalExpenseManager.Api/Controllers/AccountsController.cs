using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Api.Controllers;

// Read-only for now: added during the Authentication phase to prove and test
// per-user ownership isolation. The Accounts phase extends this controller with
// create/update/delete and balance calculation; it does not replace it.
[ApiController]
[Route("api/accounts")]
public class AccountsController(IAccountQueries accountQueries) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var accounts = await accountQueries.GetForUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(accounts));
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

        return Ok(ApiResponse<object>.Ok(account));
    }
}
