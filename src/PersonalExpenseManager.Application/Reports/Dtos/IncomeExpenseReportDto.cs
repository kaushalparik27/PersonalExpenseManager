using PersonalExpenseManager.Application.Dashboard.Dtos;

namespace PersonalExpenseManager.Application.Reports.Dtos;

public record IncomeExpenseReportDto(
    decimal TotalAmount,
    IReadOnlyList<MonthlyAmountDto> MonthlySeries,
    IReadOnlyList<CategoryAmountDto> Categories);
