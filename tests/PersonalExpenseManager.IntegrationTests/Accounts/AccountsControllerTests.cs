using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;
using PersonalExpenseManager.Infrastructure.Persistence;

namespace PersonalExpenseManager.IntegrationTests.Accounts;

public class AccountsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static CreateAccountRequest NewAccountRequest(string name = "Test Account") =>
        new(name, AccountType.Bank, "For testing", 1000m, "INR", null, 0);

    private async Task SeedTransactionAsync(
        Guid userId,
        Guid accountId,
        TransactionType type,
        decimal amount,
        Guid? transferToAccountId = null,
        bool isDeleted = false)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Transactions.Add(new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            TransferToAccountId = transferToAccountId,
            TransactionType = type,
            Amount = amount,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsDeleted = isDeleted,
            DeletedAt = isDeleted ? DateTime.UtcNow : null,
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsAccountWithCurrentBalanceEqualToOpeningBalance()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var response = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(1000m, body!.Data!.OpeningBalance);
        Assert.Equal(1000m, body.Data.CurrentBalance);
        Assert.True(body.Data.IsActive);
    }

    [Fact]
    public async Task Create_InvalidCurrency_ReturnsUnprocessableEntity()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var request = new CreateAccountRequest("Bad Currency", AccountType.Cash, null, 0m, "rupees", null, 0);

        var response = await client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CurrentBalance_ReflectsIncomeExpenseTransferAndAdjustment()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var createResponse = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var account = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var otherCreateResponse = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest("Other Account"));
        var otherAccount = (await otherCreateResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        // Opening 1000 + Income 500 - Expense 200 + TransferIn 150 - TransferOut 100 + Adjustment -50 = 1300
        await SeedTransactionAsync(userId, account.Id, TransactionType.Income, 500m);
        await SeedTransactionAsync(userId, account.Id, TransactionType.Expense, 200m);
        await SeedTransactionAsync(userId, otherAccount.Id, TransactionType.Transfer, 150m, transferToAccountId: account.Id);
        await SeedTransactionAsync(userId, account.Id, TransactionType.Transfer, 100m, transferToAccountId: otherAccount.Id);
        await SeedTransactionAsync(userId, account.Id, TransactionType.Adjustment, -50m);
        // Soft-deleted transactions must not affect the balance.
        await SeedTransactionAsync(userId, account.Id, TransactionType.Expense, 9999m, isDeleted: true);

        var response = await client.GetAsync($"/api/accounts/{account.Id}");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();

        Assert.Equal(1300m, body!.Data!.CurrentBalance);
    }

    [Fact]
    public async Task Update_ChangesPersistedFields()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var account = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var updateRequest = new UpdateAccountRequest("Renamed", AccountType.CreditCard, "Updated", 2000m, "USD", "card", 5, true);
        var response = await client.PutAsJsonAsync($"/api/accounts/{account.Id}", updateRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Renamed", body!.Data!.Name);
        Assert.Equal("CreditCard", body.Data.AccountType);
        Assert.Equal(2000m, body.Data.OpeningBalance);
        Assert.Equal("USD", body.Data.Currency);
    }

    [Fact]
    public async Task Archive_SetsIsActiveFalse()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var account = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var archiveResponse = await client.PatchAsync($"/api/accounts/{account.Id}/archive", null);
        var getResponse = await client.GetAsync($"/api/accounts/{account.Id}");
        var body = await getResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>();

        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);
        Assert.False(body!.Data!.IsActive);
    }

    [Fact]
    public async Task Delete_AccountWithNoTransactions_Succeeds()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var account = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"/api/accounts/{account.Id}");
        var getResponse = await client.GetAsync($"/api/accounts/{account.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_AccountWithTransactions_ReturnsConflictAndKeepsAccount()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var account = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;
        await SeedTransactionAsync(userId, account.Id, TransactionType.Income, 100m);

        var deleteResponse = await client.DeleteAsync($"/api/accounts/{account.Id}");
        var getResponse = await client.GetAsync($"/api/accounts/{account.Id}");

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task UserA_CannotUpdateUserBAccount()
    {
        var (clientB, userBId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await clientB.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var accountB = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var updateRequest = new UpdateAccountRequest("Hijacked", AccountType.Cash, null, 0m, "INR", null, 0, true);
        var response = await clientA.PutAsJsonAsync($"/api/accounts/{accountB.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _ = userBId;
    }

    [Fact]
    public async Task UserA_CannotArchiveUserBAccount()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await clientB.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var accountB = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var response = await clientA.PatchAsync($"/api/accounts/{accountB.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserA_CannotDeleteUserBAccount()
    {
        var (clientB, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var createResponse = await clientB.PostAsJsonAsync("/api/accounts", NewAccountRequest());
        var accountB = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var response = await clientA.DeleteAsync($"/api/accounts/{accountB.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
