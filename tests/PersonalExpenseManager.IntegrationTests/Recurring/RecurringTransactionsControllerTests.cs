using System.Net;
using System.Net.Http.Json;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.RecurringTransactions.Dtos;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.IntegrationTests.Recurring;

public class RecurringTransactionsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static CreateAccountRequest NewAccountRequest(string name = "Test Account") =>
        new(name, AccountType.Bank, null, 1000m, "INR", null, 0);

    private static CreateCategoryRequest NewCategoryRequest(string name = "Rent") => new(name, null);

    private async Task<(Guid AccountId, Guid CategoryId)> SeedAccountAndExpenseCategoryAsync(HttpClient client)
    {
        var accountResponse = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var account = (await accountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var categoryResponse = await client.PostAsJsonAsync("/api/expense-categories", NewCategoryRequest());
        var category = (await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        return (account.Id, category.Id);
    }

    [Fact]
    public async Task CreateExpense_ValidRequest_ReturnsCreatedWithNextOccurrenceEqualToStartDate()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);

        var request = new CreateRecurringExpenseRequest(accountId, 1500m, categoryId, null, "Rent", "Monthly", new DateOnly(2026, 1, 1), null);
        var response = await client.PostAsJsonAsync("/api/recurring/expense", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Expense", body!.Data!.TransactionType);
        Assert.Equal("Monthly", body.Data.Frequency);
        Assert.Equal(new DateOnly(2026, 1, 1), body.Data.NextOccurrenceDate);
        Assert.True(body.Data.IsActive);
    }

    [Fact]
    public async Task CreateExpense_InvalidFrequency_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);

        var request = new CreateRecurringExpenseRequest(accountId, 1500m, categoryId, null, "Rent", "Fortnightly", new DateOnly(2026, 1, 1), null);
        var response = await client.PostAsJsonAsync("/api/recurring/expense", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_AnotherUsersAccount_ReturnsUnprocessableEntity()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountBId, _) = await SeedAccountAndExpenseCategoryAsync(clientB);

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (_, categoryAId) = await SeedAccountAndExpenseCategoryAsync(clientA);

        var request = new CreateRecurringExpenseRequest(accountBId, 100m, categoryAId, null, null, "Monthly", new DateOnly(2026, 1, 1), null);
        var response = await clientA.PostAsJsonAsync("/api/recurring/expense", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOrderedByNextOccurrenceDate()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);

        await client.PostAsJsonAsync("/api/recurring/expense",
            new CreateRecurringExpenseRequest(accountId, 100m, categoryId, null, "Later", "Monthly", new DateOnly(2026, 6, 1), null));
        await client.PostAsJsonAsync("/api/recurring/expense",
            new CreateRecurringExpenseRequest(accountId, 100m, categoryId, null, "Sooner", "Monthly", new DateOnly(2026, 1, 1), null));

        var response = await client.GetAsync("/api/recurring");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<RecurringTransactionDto>>>();

        Assert.Equal(2, body!.Data!.Count);
        Assert.Equal("Sooner", body.Data[0].Description);
        Assert.Equal("Later", body.Data[1].Description);
    }

    [Fact]
    public async Task Update_ChangeType_IsRejected()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/recurring/expense",
            new CreateRecurringExpenseRequest(accountId, 100m, categoryId, null, "Rent", "Monthly", new DateOnly(2026, 1, 1), null));
        var recurring = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>())!.Data!;

        // No income/expense category at all — invalid for an Expense-type template.
        var updateRequest = new UpdateRecurringTransactionRequest(accountId, 200m, null, null, null, null, null, "Rent", "Monthly", new DateOnly(2026, 1, 1), null);
        var response = await client.PutAsJsonAsync($"/api/recurring/{recurring.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Update_ValidChange_PersistsAndAdvancesNextOccurrenceWhenStartDateMovesForward()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/recurring/expense",
            new CreateRecurringExpenseRequest(accountId, 100m, categoryId, null, "Rent", "Monthly", new DateOnly(2026, 1, 1), null));
        var recurring = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>())!.Data!;

        var updateRequest = new UpdateRecurringTransactionRequest(accountId, 250m, null, categoryId, null, null, null, "Rent (raised)", "Monthly", new DateOnly(2026, 3, 1), null);
        var response = await client.PutAsJsonAsync($"/api/recurring/{recurring.Id}", updateRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(250m, body!.Data!.Amount);
        Assert.Equal(new DateOnly(2026, 3, 1), body.Data.NextOccurrenceDate);
    }

    [Fact]
    public async Task Pause_ThenResume_TogglesIsActive()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/recurring/expense",
            new CreateRecurringExpenseRequest(accountId, 100m, categoryId, null, "Rent", "Monthly", new DateOnly(2026, 1, 1), null));
        var recurring = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>())!.Data!;

        var pauseResponse = await client.PatchAsync($"/api/recurring/{recurring.Id}/pause", null);
        var pauseBody = await pauseResponse.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>();
        Assert.False(pauseBody!.Data!.IsActive);

        var resumeResponse = await client.PatchAsync($"/api/recurring/{recurring.Id}/resume", null);
        var resumeBody = await resumeResponse.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>();
        Assert.True(resumeBody!.Data!.IsActive);
    }

    [Fact]
    public async Task Delete_RemovesRecurringTransaction()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/recurring/expense",
            new CreateRecurringExpenseRequest(accountId, 100m, categoryId, null, "Rent", "Monthly", new DateOnly(2026, 1, 1), null));
        var recurring = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"/api/recurring/{recurring.Id}");
        var getResponse = await client.GetAsync($"/api/recurring/{recurring.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task UserA_CannotSeeOrModifyUserBRecurringTransaction()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountBId, categoryBId) = await SeedAccountAndExpenseCategoryAsync(clientB);
        var createResponse = await clientB.PostAsJsonAsync("/api/recurring/expense",
            new CreateRecurringExpenseRequest(accountBId, 100m, categoryBId, null, "Rent", "Monthly", new DateOnly(2026, 1, 1), null));
        var recurringB = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<RecurringTransactionDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var getResponse = await clientA.GetAsync($"/api/recurring/{recurringB.Id}");
        var pauseResponse = await clientA.PatchAsync($"/api/recurring/{recurringB.Id}/pause", null);
        var deleteResponse = await clientA.DeleteAsync($"/api/recurring/{recurringB.Id}");
        var listResponse = await clientA.GetAsync("/api/recurring");
        var listBody = await listResponse.Content.ReadFromJsonAsync<ApiResponse<List<RecurringTransactionDto>>>();

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, pauseResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Empty(listBody!.Data!);
    }
}
