namespace PersonalExpenseManager.Application.Categories;

// Creates a newly-registered user's starting set of expense categories, per the
// project spec's "may optionally create default categories" registration behavior.
// The user can rename, reorder, or archive these afterward like any other category.
public interface IDefaultCategorySeeder
{
    Task SeedDefaultExpenseCategoriesAsync(Guid userId, CancellationToken cancellationToken = default);
}
