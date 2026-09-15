using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Dashboard.Dtos;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;
using PersonalExpenseManager.Infrastructure.Persistence;

namespace PersonalExpenseManager.IntegrationTests.Dashboard;

public class ReportsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static async Task<Guid> CreateAccountAsync(HttpClient client, string name = "Bank")
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

    private static async Task<Guid> GetCategoryIdAsync(HttpClient client, string name)
    {
        var response = await client.GetAsync("/api/expense-categories");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();
        return body!.Data!.Single(c => c.Name == name).Id;
    }

    private async Task SeedTransactionAsync(
        Guid userId, Guid accountId, TransactionType type, decimal amount, DateOnly date, Guid? categoryId = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Transactions.Add(new Transaction
        {
            UserId = userId,
            AccountId = accountId,
            TransactionType = type,
            Amount = amount,
            TransactionDate = date,
            ExpenseCategoryId = type == TransactionType.Expense ? categoryId : null,
            IncomeCategoryId = type == TransactionType.Income ? categoryId : null,
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDashboard_NoTransactions_ReturnsZerosAndNotEnoughDataInsight()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        await CreateAccountAsync(client);

        var response = await client.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0m, body!.Data!.ThisMonthIncome);
        Assert.Equal(0m, body.Data.ThisMonthExpense);
        Assert.Equal(1000m, body.Data.TotalBalance);
        Assert.Contains("Not enough data yet.", body.Data.Insights);
    }

    [Fact]
    public async Task GetDashboard_ThisMonthTransactions_ComputesTotalsAndSavingsRate()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, accountId, TransactionType.Income, 10_000m, today);
        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 4_000m, today, categoryId);

        var response = await client.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        Assert.Equal(10_000m, body!.Data!.ThisMonthIncome);
        Assert.Equal(4_000m, body.Data.ThisMonthExpense);
        Assert.Equal(6_000m, body.Data.NetCashFlow);
        Assert.Equal(60.0m, body.Data.SavingsRatePercent);
        Assert.Contains("Your savings rate is 60%.", body.Data.Insights);
    }

    [Fact]
    public async Task GetDashboard_ExpenseGrowth_ComparesAgainstPreviousMonth()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-1).AddDays(5);

        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 1_000m, lastMonth, categoryId);
        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 1_500m, today, categoryId);

        var response = await client.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        // (1500 - 1000) / 1000 * 100 = 50%
        Assert.Equal(50.0m, body!.Data!.ExpenseGrowthPercent);
    }

    [Fact]
    public async Task GetDashboard_TopExpenseCategories_OrderedByAmountDescending()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var foodId = await GetFoodExpenseCategoryIdAsync(client);
        var transportId = await GetCategoryIdAsync(client, "Transportation");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 500m, today, transportId);
        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 2_000m, today, foodId);

        var response = await client.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        Assert.Equal("Food", body!.Data!.TopExpenseCategories[0].CategoryName);
        Assert.Equal(2_000m, body.Data.TopExpenseCategories[0].Amount);
        Assert.Contains("Food is your largest expense category.", body.Data.Insights);
    }

    [Fact]
    public async Task GetDashboard_BalanceByAccountType_GroupsAcrossAccounts()
    {
        var (client, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        await CreateAccountAsync(client, "Bank A");
        await CreateAccountAsync(client, "Bank B");

        var response = await client.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        var bankGroup = body!.Data!.BalanceByAccountType.Single(b => b.AccountType == "Bank");
        Assert.Equal(2000m, bankGroup.Balance);
        Assert.Equal(2000m, body.Data.TotalBalance);
    }

    [Fact]
    public async Task GetDashboard_RecentTransactions_NewestFirst()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 100m, today.AddDays(-2), categoryId);
        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 200m, today, categoryId);

        var response = await client.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        Assert.Equal(200m, body!.Data!.RecentTransactions[0].Amount);
    }

    [Fact]
    public async Task GetDashboard_HiddenAccount_ExcludedFromTotalsCategoriesAndTrend()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var visibleAccountId = await CreateAccountAsync(client, "Visible");
        var hiddenAccountId = await CreateAccountAsync(client, "Hidden");
        var categoryId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, visibleAccountId, TransactionType.Income, 5_000m, today);
        await SeedTransactionAsync(userId, visibleAccountId, TransactionType.Expense, 1_000m, today, categoryId);
        // These would double every total below if the hidden account weren't excluded.
        await SeedTransactionAsync(userId, hiddenAccountId, TransactionType.Income, 50_000m, today);
        await SeedTransactionAsync(userId, hiddenAccountId, TransactionType.Expense, 20_000m, today, categoryId);

        await client.PatchAsync($"/api/accounts/{hiddenAccountId}/archive", null);

        var response = await client.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        Assert.Equal(5_000m, body!.Data!.ThisMonthIncome);
        Assert.Equal(1_000m, body.Data.ThisMonthExpense);
        Assert.Equal(1_000m, body.Data.TopExpenseCategories[0].Amount);
    }

    [Fact]
    public async Task UserA_DashboardNeverIncludesUserBData()
    {
        var (clientB, userBId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountB = await CreateAccountAsync(clientB, "User B Bank");
        var categoryB = await GetFoodExpenseCategoryIdAsync(clientB);
        await SeedTransactionAsync(userBId, accountB, TransactionType.Income, 999_999m, DateOnly.FromDateTime(DateTime.UtcNow));

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        await CreateAccountAsync(clientA, "User A Bank");

        var response = await clientA.GetAsync("/api/reports/dashboard");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardDto>>();

        Assert.Equal(0m, body!.Data!.ThisMonthIncome);
        Assert.Equal(1000m, body.Data.TotalBalance);
    }

    [Fact]
    public async Task AnonymousUser_CannotAccessDashboard()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/reports/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
