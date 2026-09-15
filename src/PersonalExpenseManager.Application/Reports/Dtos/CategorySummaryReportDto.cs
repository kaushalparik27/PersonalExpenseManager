namespace PersonalExpenseManager.Application.Reports.Dtos;

public record CategorySummarySubCategoryDto(Guid Id, string Name, decimal Amount);

public record CategorySummaryCategoryDto(
    Guid Id,
    string Name,
    decimal Amount,
    IReadOnlyList<CategorySummarySubCategoryDto> SubCategories);

public record CategorySummaryReportDto(
    IReadOnlyList<CategorySummaryCategoryDto> ExpenseCategories,
    IReadOnlyList<CategorySummaryCategoryDto> IncomeCategories);
