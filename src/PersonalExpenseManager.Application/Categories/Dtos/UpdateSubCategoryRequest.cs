namespace PersonalExpenseManager.Application.Categories.Dtos;

public record UpdateSubCategoryRequest(string Name, Guid CategoryId, bool IsArchived, string? Icon = null);
