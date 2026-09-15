using System.Net;
using System.Net.Http.Json;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.IntegrationTests.Categories;

// Income categories share the exact same command/query logic as expense
// categories (see ExpenseCategoriesControllerTests for the full CRUD/ownership
// coverage) — this file just proves the income-specific routes and DI wiring
// work, rather than re-testing the shared logic in duplicate.
public class IncomeCategoriesControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetAll_NoDefaultSeeding_ReturnsEmptyList()
    {
        // Unlike expense categories, income categories are not auto-seeded on registration.
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var response = await client.GetAsync("/api/income-categories");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(body!.Data!);
    }

    [Fact]
    public async Task CreateCategory_WithSubCategory_ReturnsNestedShape()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var createResponse = await client.PostAsJsonAsync("/api/income-categories", new CreateCategoryRequest("Business", null));
        var category = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var subResponse = await client.PostAsJsonAsync($"/api/income-categories/{category.Id}/subcategories", new CreateSubCategoryRequest("Client A"));

        var getResponse = await client.GetAsync("/api/income-categories");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();

        Assert.Equal(HttpStatusCode.OK, subResponse.StatusCode);
        Assert.Single(body!.Data!.Single(c => c.Id == category.Id).SubCategories);
    }

    [Fact]
    public async Task AnonymousUser_CannotListIncomeCategories()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/income-categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
