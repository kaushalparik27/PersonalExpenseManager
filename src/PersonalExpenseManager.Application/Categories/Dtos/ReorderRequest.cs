namespace PersonalExpenseManager.Application.Categories.Dtos;

public record ReorderRequest(IReadOnlyList<Guid> OrderedIds);
