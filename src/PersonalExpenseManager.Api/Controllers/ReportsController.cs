using Microsoft.AspNetCore.Mvc;
using PersonalExpenseManager.Api.Common;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Dashboard;
using PersonalExpenseManager.Application.Dashboard.Dtos;
using PersonalExpenseManager.Application.Reports;
using PersonalExpenseManager.Application.Reports.Dtos;

namespace PersonalExpenseManager.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IDashboardQueries dashboardQueries, IReportsQueries reportsQueries) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await dashboardQueries.GetDashboardAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<DashboardDto>.Ok(dashboard));
    }

    [HttpGet("balance-trend")]
    public async Task<IActionResult> GetBalanceTrend(
        [FromQuery] BalanceTrendGranularity granularity = BalanceTrendGranularity.Monthly, CancellationToken cancellationToken = default)
    {
        var trend = await dashboardQueries.GetBalanceTrendAsync(User.GetUserId(), granularity, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BalanceTrendPointDto>>.Ok(trend));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await dashboardQueries.GetPeriodSummariesAsync(User.GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PeriodSummaryDto>>.Ok(summary));
    }

    [HttpGet("income")]
    public async Task<IActionResult> GetIncomeReport([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveDateRange(fromDate, toDate);
        var report = await reportsQueries.GetIncomeReportAsync(User.GetUserId(), from, to, cancellationToken);
        return Ok(ApiResponse<IncomeExpenseReportDto>.Ok(report));
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenseReport([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveDateRange(fromDate, toDate);
        var report = await reportsQueries.GetExpenseReportAsync(User.GetUserId(), from, to, cancellationToken);
        return Ok(ApiResponse<IncomeExpenseReportDto>.Ok(report));
    }

    [HttpGet("cash-flow")]
    public async Task<IActionResult> GetCashFlowReport([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveDateRange(fromDate, toDate);
        var report = await reportsQueries.GetCashFlowReportAsync(User.GetUserId(), from, to, cancellationToken);
        return Ok(ApiResponse<CashFlowReportDto>.Ok(report));
    }

    [HttpGet("category-summary")]
    public async Task<IActionResult> GetCategorySummary([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveDateRange(fromDate, toDate);
        var report = await reportsQueries.GetCategorySummaryAsync(User.GetUserId(), from, to, cancellationToken);
        return Ok(ApiResponse<CategorySummaryReportDto>.Ok(report));
    }

    [HttpGet("account-summary")]
    public async Task<IActionResult> GetAccountSummary([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveDateRange(fromDate, toDate);
        var report = await reportsQueries.GetAccountSummaryAsync(User.GetUserId(), from, to, cancellationToken);
        return Ok(ApiResponse<AccountSummaryReportDto>.Ok(report));
    }

    [HttpGet("monthly-trend")]
    public async Task<IActionResult> GetMonthlyTrend([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken cancellationToken)
    {
        var (from, to) = ResolveDateRange(fromDate, toDate);
        var report = await reportsQueries.GetMonthlyTrendAsync(User.GetUserId(), from, to, cancellationToken);
        return Ok(ApiResponse<MonthlyTrendReportDto>.Ok(report));
    }

    // Spec default: current month, when no explicit range is supplied.
    private static (DateOnly From, DateOnly To) ResolveDateRange(DateOnly? fromDate, DateOnly? toDate)
    {
        if (fromDate is not null && toDate is not null)
        {
            return (fromDate.Value, toDate.Value);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        return (fromDate ?? monthStart, toDate ?? monthEnd);
    }
}
