using PersonalExpenseManager.Application.Dashboard.Dtos;

namespace PersonalExpenseManager.Application.Reports.Dtos;

public record CashFlowReportDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetCashFlow,
    decimal? SavingsRatePercent,
    IReadOnlyList<MonthlyAmountDto> MonthlySeries);
