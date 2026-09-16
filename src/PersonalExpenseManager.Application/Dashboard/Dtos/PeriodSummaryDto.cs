namespace PersonalExpenseManager.Application.Dashboard.Dtos;

public record PeriodSummaryDto(SummaryPeriod Period, decimal Income, decimal Expense);
