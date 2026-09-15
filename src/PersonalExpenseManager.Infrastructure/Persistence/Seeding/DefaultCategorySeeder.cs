using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Domain.Entities;

namespace PersonalExpenseManager.Infrastructure.Persistence.Seeding;

public class DefaultCategorySeeder(ApplicationDbContext db) : IDefaultCategorySeeder
{
    // Icon values must match a key in the frontend's CATEGORY_ICON_ENTRIES
    // (client/src/lib/categoryIcons.ts) — kept in sync manually since the icon
    // registry lives client-side and the backend just stores an opaque string.
    private static readonly (string Name, string Icon)[] DefaultExpenseCategories =
    [
        ("Food", "Utensils"),
        ("Transportation", "Car"),
        ("Utilities", "Lightbulb"),
        ("Home", "Home"),
        ("Healthcare", "HeartPulse"),
        ("Entertainment", "Clapperboard"),
        ("Shopping", "ShoppingBag"),
        ("Education", "GraduationCap"),
        ("Insurance", "Shield"),
        ("Investment", "TrendingUp"),
        ("Other", "Tag"),
    ];

    public async Task SeedDefaultExpenseCategoriesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = DefaultExpenseCategories.Select((c, index) => new ExpenseCategory
        {
            UserId = userId,
            Name = c.Name,
            Icon = c.Icon,
            DisplayOrder = index,
        });

        db.ExpenseCategories.AddRange(categories);
        await db.SaveChangesAsync(cancellationToken);
    }
}
