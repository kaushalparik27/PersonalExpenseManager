using System.Net;
using System.Net.Http.Json;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.IntegrationTests.Categories;

public class ExpenseCategoriesControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    [Fact]
    public async Task GetAll_AfterRegistration_ReturnsDefaultSeededCategories()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var response = await client.GetAsync("/api/expense-categories");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(11, body!.Data!.Count);
        Assert.Contains(body.Data, c => c.Name == "Food");
    }

    [Fact]
    public async Task CreateCategory_ValidName_Succeeds()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var name = UniqueName("Travel");

        var response = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(name, null));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(name, body!.Data!.Name);
        Assert.False(body.Data.IsArchived);
        Assert.Empty(body.Data.SubCategories);
    }

    [Fact]
    public async Task CreateCategory_DuplicateName_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        // "Food" already exists from default-category seeding on registration.
        var response = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest("Food", null));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_RenamesAndArchives()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("Old"), null));
        var category = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var newName = UniqueName("New");
        var response = await client.PutAsJsonAsync($"/api/expense-categories/{category.Id}", new UpdateCategoryRequest(newName, "star", true));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(newName, body!.Data!.Name);
        Assert.True(body.Data.IsArchived);
    }

    [Fact]
    public async Task ArchiveCategory_SetsIsArchivedTrue()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("ToArchive"), null));
        var category = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var archiveResponse = await client.PatchAsync($"/api/expense-categories/{category.Id}/archive", null);
        var getResponse = await client.GetAsync("/api/expense-categories");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);
        Assert.True(body!.Data!.Single(c => c.Id == category.Id).IsArchived);
    }

    [Fact]
    public async Task CreateSubCategory_ThenReorder_PersistsNewOrder()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("Parent"), null));
        var category = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var sub1Response = await client.PostAsJsonAsync($"/api/expense-categories/{category.Id}/subcategories", new CreateSubCategoryRequest("Alpha"));
        var sub1 = (await sub1Response.Content.ReadFromJsonAsync<ApiResponse<SubCategoryDto>>())!.Data!;
        var sub2Response = await client.PostAsJsonAsync($"/api/expense-categories/{category.Id}/subcategories", new CreateSubCategoryRequest("Beta"));
        var sub2 = (await sub2Response.Content.ReadFromJsonAsync<ApiResponse<SubCategoryDto>>())!.Data!;

        Assert.Equal(0, sub1.DisplayOrder);
        Assert.Equal(1, sub2.DisplayOrder);

        var reorderResponse = await client.PatchAsJsonAsync(
            $"/api/expense-categories/{category.Id}/subcategories/reorder",
            new ReorderRequest([sub2.Id, sub1.Id]));

        var getResponse = await client.GetAsync("/api/expense-categories");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();
        var reloaded = body!.Data!.Single(c => c.Id == category.Id);

        Assert.Equal(HttpStatusCode.NoContent, reorderResponse.StatusCode);
        Assert.Equal(sub2.Id, reloaded.SubCategories[0].Id);
        Assert.Equal(sub1.Id, reloaded.SubCategories[1].Id);
    }

    [Fact]
    public async Task UpdateSubCategory_MoveToAnotherCategory_ChangesParentAndAppendsAtEnd()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var categoryAResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("CategoryA"), null));
        var categoryA = (await categoryAResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;
        var categoryBResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("CategoryB"), null));
        var categoryB = (await categoryBResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var subResponse = await client.PostAsJsonAsync($"/api/expense-categories/{categoryA.Id}/subcategories", new CreateSubCategoryRequest("Movable"));
        var sub = (await subResponse.Content.ReadFromJsonAsync<ApiResponse<SubCategoryDto>>())!.Data!;
        // categoryB already has one subcategory, so the moved one should land after it (DisplayOrder 1).
        await client.PostAsJsonAsync($"/api/expense-categories/{categoryB.Id}/subcategories", new CreateSubCategoryRequest("Existing"));

        var moveResponse = await client.PutAsJsonAsync(
            $"/api/expense-categories/subcategories/{sub.Id}",
            new UpdateSubCategoryRequest("Movable", categoryB.Id, false));
        var moved = (await moveResponse.Content.ReadFromJsonAsync<ApiResponse<SubCategoryDto>>())!.Data!;

        var getResponse = await client.GetAsync("/api/expense-categories");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();
        var reloadedA = body!.Data!.Single(c => c.Id == categoryA.Id);
        var reloadedB = body.Data!.Single(c => c.Id == categoryB.Id);

        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
        Assert.Equal(1, moved.DisplayOrder);
        Assert.DoesNotContain(reloadedA.SubCategories, s => s.Id == sub.Id);
        Assert.Contains(reloadedB.SubCategories, s => s.Id == sub.Id);
    }

    [Fact]
    public async Task UpdateSubCategory_MoveToCategoryWithDuplicateName_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var categoryAResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("CategoryA"), null));
        var categoryA = (await categoryAResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;
        var categoryBResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("CategoryB"), null));
        var categoryB = (await categoryBResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var subResponse = await client.PostAsJsonAsync($"/api/expense-categories/{categoryA.Id}/subcategories", new CreateSubCategoryRequest("Groceries"));
        var sub = (await subResponse.Content.ReadFromJsonAsync<ApiResponse<SubCategoryDto>>())!.Data!;
        await client.PostAsJsonAsync($"/api/expense-categories/{categoryB.Id}/subcategories", new CreateSubCategoryRequest("Groceries"));

        var moveResponse = await client.PutAsJsonAsync(
            $"/api/expense-categories/subcategories/{sub.Id}",
            new UpdateSubCategoryRequest("Groceries", categoryB.Id, false));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, moveResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateSubCategory_MoveToAnotherUsersCategory_ReturnsUnprocessableEntity()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var categoryBResponse = await clientB.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("PrivateB"), null));
        var categoryB = (await categoryBResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var categoryAResponse = await clientA.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("CategoryA"), null));
        var categoryA = (await categoryAResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;
        var subResponse = await clientA.PostAsJsonAsync($"/api/expense-categories/{categoryA.Id}/subcategories", new CreateSubCategoryRequest("Mine"));
        var sub = (await subResponse.Content.ReadFromJsonAsync<ApiResponse<SubCategoryDto>>())!.Data!;

        var moveResponse = await clientA.PutAsJsonAsync(
            $"/api/expense-categories/subcategories/{sub.Id}",
            new UpdateSubCategoryRequest("Mine", categoryB.Id, false));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, moveResponse.StatusCode);
    }

    [Fact]
    public async Task CreateSubCategory_DuplicateNameWithinCategory_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("Parent"), null));
        var category = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        await client.PostAsJsonAsync($"/api/expense-categories/{category.Id}/subcategories", new CreateSubCategoryRequest("Groceries"));
        var response = await client.PostAsJsonAsync($"/api/expense-categories/{category.Id}/subcategories", new CreateSubCategoryRequest("Groceries"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UserA_CannotUpdateUserBCategory()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await clientB.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("Private"), null));
        var categoryB = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var response = await clientA.PutAsJsonAsync($"/api/expense-categories/{categoryB.Id}", new UpdateCategoryRequest("Hijacked", null, false));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserA_CannotAddSubCategoryToUserBCategory()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await clientB.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest(UniqueName("Private"), null));
        var categoryB = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var response = await clientA.PostAsJsonAsync($"/api/expense-categories/{categoryB.Id}/subcategories", new CreateSubCategoryRequest("Sneaky"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousUser_CannotListCategories()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/expense-categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
