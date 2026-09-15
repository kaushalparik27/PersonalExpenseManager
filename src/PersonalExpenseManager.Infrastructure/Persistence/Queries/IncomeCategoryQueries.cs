using Microsoft.EntityFrameworkCore;
using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Domain.Entities;

namespace PersonalExpenseManager.Infrastructure.Persistence.Queries;

public class IncomeCategoryQueries(ApplicationDbContext db) : IIncomeCategoryQueries
{
    public async Task<IReadOnlyList<CategoryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await db.IncomeCategories
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.SubCategories.OrderBy(s => s.DisplayOrder))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        return categories.Select(ToDto).ToList();
    }

    internal static CategoryDto ToDto(IncomeCategory c) => new(
        c.Id,
        c.Name,
        c.Icon,
        c.DisplayOrder,
        c.IsArchived,
        c.SubCategories.Select(s => new SubCategoryDto(s.Id, s.Name, s.DisplayOrder, s.IsArchived)).ToList());
}
