namespace PersonalExpenseManager.Application.Categories.Dtos;

public record SubCategoryDto(Guid Id, string Name, int DisplayOrder, bool IsArchived);
