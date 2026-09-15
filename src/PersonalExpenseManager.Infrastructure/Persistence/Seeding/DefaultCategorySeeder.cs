using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Domain.Entities;

namespace PersonalExpenseManager.Infrastructure.Persistence.Seeding;

public class DefaultCategorySeeder(ApplicationDbContext db) : IDefaultCategorySeeder
{
    private static readonly string[] DefaultExpenseCategoryNames =
    [
        "Food", "Transportation", "Utilities", "Home", "Healthcare",
        "Entertainment", "Shopping", "Education", "Insurance", "Investment", "Other",
    ];

    public async Task SeedDefaultExpenseCategoriesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = DefaultExpenseCategoryNames.Select((name, index) => new ExpenseCategory
        {
            UserId = userId,
            Name = name,
            DisplayOrder = index,
        });

        db.ExpenseCategories.AddRange(categories);
        await db.SaveChangesAsync(cancellationToken);
    }
}
