using PersonalExpenseManager.Application.Categories.Dtos;

namespace PersonalExpenseManager.Application.Categories;

public interface IIncomeCategoryQueries
{
    Task<IReadOnlyList<CategoryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
