using PersonalExpenseManager.Application.Reports.Dtos;

namespace PersonalExpenseManager.Application.Reports;

public interface IReportsQueries
{
    Task<IncomeExpenseReportDto> GetIncomeReportAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<IncomeExpenseReportDto> GetExpenseReportAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<CashFlowReportDto> GetCashFlowReportAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<CategorySummaryReportDto> GetCategorySummaryAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<AccountSummaryReportDto> GetAccountSummaryAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);

    Task<MonthlyTrendReportDto> GetMonthlyTrendAsync(Guid userId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
}
