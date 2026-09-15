namespace PersonalExpenseManager.Application.Categories.Dtos;

public record UpdateCategoryRequest(string Name, string? Icon, bool IsArchived);
