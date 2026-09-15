namespace PersonalExpenseManager.Application.Categories.Dtos;

public record SubCategoryDto(Guid Id, string Name, string? Icon, int DisplayOrder, bool IsArchived);
