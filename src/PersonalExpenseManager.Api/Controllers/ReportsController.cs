using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Dashboard;
using PersonalExpenseManager.Application.Dashboard.Dtos;

namespace PersonalExpenseManager.Api.Controllers;

// Only the dashboard aggregation lives here for now — the Reports phase adds
// income/expense/cash-flow/category/account/monthly-trend endpoints alongside it.
[ApiController]
[Route("api/reports")]
public class ReportsController(IDashboardQueries dashboardQueries) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await dashboardQueries.GetDashboardAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<DashboardDto>.Ok(dashboard));
    }
}
