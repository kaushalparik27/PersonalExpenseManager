namespace PersonalExpenseManager.Application.Dashboard.Dtos;

public record MonthlyAmountDto(int Year, int Month, decimal Income, decimal Expense);
