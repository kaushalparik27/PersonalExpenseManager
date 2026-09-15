using System.Net;
using System.Net.Http.Json;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Transactions.Dtos;
using PersonalExpenseManager.Domain.Enums;

namespace PersonalExpenseManager.IntegrationTests.Transactions;

public class TransactionsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static async Task<Guid> CreateAccountAsync(HttpClient client, string name = "Test Account")
    {
        var response = await client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(name, AccountType.Bank, null, 1000m, "INR", null, 0));
        return (await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!.Id;
    }

    private static async Task<Guid> GetFoodExpenseCategoryIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/expense-categories");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();
        return body!.Data!.Single(c => c.Name == "Food").Id;
    }

    private static async Task<Guid> CreateIncomeCategoryAsync(HttpClient client, string name = "Salary")
    {
        var response = await client.PostAsJsonAsync("/api/income-categories", new CreateCategoryRequest(name, null));
        return (await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!.Id;
    }

    [Fact]
    public async Task CreateExpense_Valid_ReturnsResolvedNames()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client, "HDFC Bank");
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);

        var request = new CreateExpenseRequest(
            new DateOnly(2026, 1, 15), accountId, 500m, categoryId, null, "Groceries", null, null, null, ["monthly"]);

        var response = await client.PostAsJsonAsync("/api/transactions/expense", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Expense", body!.Data!.TransactionType);
        Assert.Equal("HDFC Bank", body.Data.AccountName);
        Assert.Equal("Food", body.Data.ExpenseCategoryName);
        Assert.Equal(500m, body.Data.Amount);
        Assert.Contains("monthly", body.Data.Tags);
    }

    [Fact]
    public async Task CreateExpense_UnknownAccount_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);

        var request = new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), 100m, categoryId, null, null, null, null, null, null);
        var response = await client.PostAsJsonAsync("/api/transactions/expense", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_CannotUseAnotherUsersAccount()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountBId = await CreateAccountAsync(clientB);

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var categoryAId = await GetFoodExpenseCategoryIdAsync(clientA);
        var request = new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountBId, 100m, categoryAId, null, null, null, null, null, null);

        var response = await clientA.PostAsJsonAsync("/api/transactions/expense", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateIncome_Valid_Succeeds()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await CreateIncomeCategoryAsync(client);

        var request = new CreateIncomeRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 85000m, categoryId, null, "Monthly salary", null, null, null, null);
        var response = await client.PostAsJsonAsync("/api/transactions/income", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Income", body!.Data!.TransactionType);
        Assert.Equal("Salary", body.Data.IncomeCategoryName);
    }

    [Fact]
    public async Task CreateTransfer_Valid_ReturnsBothAccountNames()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var sourceId = await CreateAccountAsync(client, "Bank");
        var destId = await CreateAccountAsync(client, "Cash");

        var request = new CreateTransferRequest(DateOnly.FromDateTime(DateTime.UtcNow), sourceId, destId, 2000m, "ATM withdrawal", null, null);
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Transfer", body!.Data!.TransactionType);
        Assert.Equal("Bank", body.Data.AccountName);
        Assert.Equal("Cash", body.Data.TransferToAccountName);
    }

    [Fact]
    public async Task CreateTransfer_SameAccount_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);

        var request = new CreateTransferRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, accountId, 100m, null, null, null);
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExpenseWithIncomeCategory_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var expenseCategoryId = await GetFoodExpenseCategoryIdAsync(client);
        var incomeCategoryId = await CreateIncomeCategoryAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            "/api/transactions/expense",
            new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 100m, expenseCategoryId, null, null, null, null, null, null));
        var transaction = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>())!.Data!;

        var badUpdate = new UpdateTransactionRequest(
            DateOnly.FromDateTime(DateTime.UtcNow), accountId, 100m, null, null, null, incomeCategoryId, null, null, null, null, null, null);
        var response = await client.PutAsJsonAsync($"/api/transactions/{transaction.Id}", badUpdate);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Update_ValidChange_Persists()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            "/api/transactions/expense",
            new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 100m, categoryId, null, "Original", null, null, null, null));
        var transaction = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>())!.Data!;

        var update = new UpdateTransactionRequest(
            transaction.TransactionDate, accountId, 250m, null, categoryId, null, null, null, "Updated", null, null, null, ["fresh"]);
        var response = await client.PutAsJsonAsync($"/api/transactions/{transaction.Id}", update);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(250m, body!.Data!.Amount);
        Assert.Equal("Updated", body.Data.Description);
        Assert.Contains("fresh", body.Data.Tags);
    }

    [Fact]
    public async Task Delete_SoftDeletes_ExcludedFromGetAndList()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            "/api/transactions/expense",
            new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 100m, categoryId, null, null, null, null, null, null));
        var transaction = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"/api/transactions/{transaction.Id}");
        var getResponse = await client.GetAsync($"/api/transactions/{transaction.Id}");
        var listResponse = await client.GetAsync("/api/transactions?pageSize=100");
        var listBody = await listResponse.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionDto>>>();

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.DoesNotContain(listBody!.Data!.Items, t => t.Id == transaction.Id);
    }

    [Fact]
    public async Task GetPaged_FiltersByTransactionType()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var expenseCategoryId = await GetFoodExpenseCategoryIdAsync(client);
        var incomeCategoryId = await CreateIncomeCategoryAsync(client);

        await client.PostAsJsonAsync("/api/transactions/expense", new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 100m, expenseCategoryId, null, null, null, null, null, null));
        await client.PostAsJsonAsync("/api/transactions/income", new CreateIncomeRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 5000m, incomeCategoryId, null, null, null, null, null, null));

        var response = await client.GetAsync("/api/transactions?transactionType=Income");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionDto>>>();

        Assert.All(body!.Data!.Items, t => Assert.Equal("Income", t.TransactionType));
        Assert.Contains(body.Data.Items, t => t.Amount == 5000m);
    }

    [Fact]
    public async Task GetPaged_SearchMatchesDescriptionAndTag()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);

        await client.PostAsJsonAsync(
            "/api/transactions/expense",
            new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 100m, categoryId, null, "Amazon purchase", null, null, null, ["online"]));
        await client.PostAsJsonAsync(
            "/api/transactions/expense",
            new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 200m, categoryId, null, "Local store", null, null, null, null));

        var byDescription = await client.GetAsync("/api/transactions?search=amazon");
        var byDescriptionBody = await byDescription.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionDto>>>();

        var byTag = await client.GetAsync("/api/transactions?search=online");
        var byTagBody = await byTag.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionDto>>>();

        Assert.Single(byDescriptionBody!.Data!.Items);
        Assert.Equal("Amazon purchase", byDescriptionBody.Data.Items[0].Description);
        Assert.Single(byTagBody!.Data!.Items);
    }

    [Fact]
    public async Task GetPaged_RespectsPageSizeAndReturnsTotalCount()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);

        for (var i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync(
                "/api/transactions/expense",
                new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountId, 10m + i, categoryId, null, $"Item {i}", null, null, null, null));
        }

        var response = await client.GetAsync("/api/transactions?page=1&pageSize=2");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionDto>>>();

        Assert.Equal(2, body!.Data!.Items.Count);
        Assert.Equal(5, body.Data.TotalCount);
        Assert.Equal(3, body.Data.TotalPages);
    }

    [Fact]
    public async Task UserA_CannotViewOrModifyUserBTransaction()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountB = await CreateAccountAsync(clientB);
        var categoryB = await GetFoodExpenseCategoryIdAsync(clientB);
        var createResponse = await clientB.PostAsJsonAsync(
            "/api/transactions/expense",
            new CreateExpenseRequest(DateOnly.FromDateTime(DateTime.UtcNow), accountB, 100m, categoryB, null, null, null, null, null, null));
        var transactionB = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var getResponse = await clientA.GetAsync($"/api/transactions/{transactionB.Id}");
        var deleteResponse = await clientA.DeleteAsync($"/api/transactions/{transactionB.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task AnonymousUser_CannotListTransactions()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/transactions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
