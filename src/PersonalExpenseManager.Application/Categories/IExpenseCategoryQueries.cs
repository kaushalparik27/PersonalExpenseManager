using PersonalExpenseManager.Application.Categories.Dtos;

namespace PersonalExpenseManager.Application.Categories;

public interface IExpenseCategoryQueries
{
    Task<IReadOnlyList<CategoryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
