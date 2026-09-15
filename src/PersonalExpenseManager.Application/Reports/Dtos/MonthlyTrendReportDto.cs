using PersonalExpenseManager.Application.Dashboard.Dtos;

namespace PersonalExpenseManager.Application.Reports.Dtos;

public record MonthlyTrendReportDto(IReadOnlyList<MonthlyAmountDto> MonthlySeries);
