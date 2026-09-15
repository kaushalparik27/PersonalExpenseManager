namespace PersonalExpenseManager.Application.Categories.Dtos;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Icon,
    int DisplayOrder,
    bool IsArchived,
    IReadOnlyList<SubCategoryDto> SubCategories);
