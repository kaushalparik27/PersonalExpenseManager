using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Reports.Dtos;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;
using PersonalExpenseManager.Infrastructure.Persistence;

namespace PersonalExpenseManager.IntegrationTests.Reports;

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

    private static async Task<Guid> CreateExpenseSubCategoryAsync(HttpClient client, Guid categoryId, string name)
    {
        var response = await client.PostAsJsonAsync($"/api/expense-categories/{categoryId}/subcategories", new CreateSubCategoryRequest(name));
        return (await response.Content.ReadFromJsonAsync<ApiResponse<SubCategoryDto>>())!.Data!.Id;
    }

    private static async Task<Guid> CreateIncomeCategoryAsync(HttpClient client, string name = "Salary")
    {
        var response = await client.PostAsJsonAsync("/api/income-categories", new CreateCategoryRequest(name, null));
        return (await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!.Id;
    }

    private async Task SeedTransactionAsync(
        Guid userId, Guid accountId, TransactionType type, decimal amount, DateOnly date,
        Guid? categoryId = null, Guid? subCategoryId = null, Guid? transferToAccountId = null)
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
            TransferToAccountId = transferToAccountId,
            ExpenseCategoryId = type == TransactionType.Expense ? categoryId : null,
            ExpenseSubCategoryId = type == TransactionType.Expense ? subCategoryId : null,
            IncomeCategoryId = type == TransactionType.Income ? categoryId : null,
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetIncomeReport_ReturnsTotalAndCategoryBreakdown()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var salaryId = await CreateIncomeCategoryAsync(client, "Salary");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, accountId, TransactionType.Income, 5000m, today, salaryId);
        await SeedTransactionAsync(userId, accountId, TransactionType.Income, 2000m, today, salaryId);

        var response = await client.GetAsync("/api/reports/income");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IncomeExpenseReportDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(7000m, body!.Data!.TotalAmount);
        Assert.Equal("Salary", body.Data.Categories[0].CategoryName);
        Assert.Equal(7000m, body.Data.Categories[0].Amount);
    }

    [Fact]
    public async Task GetExpenseReport_ReturnsTotalAndCategoryBreakdown()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var foodId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 300m, today, foodId);

        var response = await client.GetAsync("/api/reports/expenses");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IncomeExpenseReportDto>>();

        Assert.Equal(300m, body!.Data!.TotalAmount);
        Assert.Equal("Food", body.Data.Categories[0].CategoryName);
    }

    [Fact]
    public async Task GetCashFlow_ComputesNetCashFlowAndSavingsRate()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var foodId = await GetFoodExpenseCategoryIdAsync(client);
        var salaryId = await CreateIncomeCategoryAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, accountId, TransactionType.Income, 10_000m, today, salaryId);
        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 3_000m, today, foodId);

        var response = await client.GetAsync("/api/reports/cash-flow");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CashFlowReportDto>>();

        Assert.Equal(10_000m, body!.Data!.TotalIncome);
        Assert.Equal(3_000m, body.Data.TotalExpense);
        Assert.Equal(7_000m, body.Data.NetCashFlow);
        Assert.Equal(70.0m, body.Data.SavingsRatePercent);
    }

    [Fact]
    public async Task GetCategorySummary_IncludesSubCategoryDrilldown()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var foodId = await GetFoodExpenseCategoryIdAsync(client);
        var groceriesId = await CreateExpenseSubCategoryAsync(client, foodId, "Groceries");
        var restaurantsId = await CreateExpenseSubCategoryAsync(client, foodId, "Restaurants");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 1_200m, today, foodId, groceriesId);
        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 800m, today, foodId, restaurantsId);

        var response = await client.GetAsync("/api/reports/category-summary");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CategorySummaryReportDto>>();

        var food = body!.Data!.ExpenseCategories.Single(c => c.Name == "Food");
        Assert.Equal(2_000m, food.Amount);
        Assert.Equal(2, food.SubCategories.Count);
        Assert.Equal(1_200m, food.SubCategories.Single(s => s.Name == "Groceries").Amount);
        Assert.Equal(800m, food.SubCategories.Single(s => s.Name == "Restaurants").Amount);
        Assert.Empty(body.Data.IncomeCategories);
    }

    [Fact]
    public async Task GetAccountSummary_ComputesPeriodNetChangePerAccount()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var sourceId = await CreateAccountAsync(client, "Source");
        var destId = await CreateAccountAsync(client, "Destination");
        var foodId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedTransactionAsync(userId, sourceId, TransactionType.Expense, 500m, today, foodId);
        await SeedTransactionAsync(userId, sourceId, TransactionType.Transfer, 200m, today, transferToAccountId: destId);

        var response = await client.GetAsync("/api/reports/account-summary");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AccountSummaryReportDto>>();

        var source = body!.Data!.Accounts.Single(a => a.AccountName == "Source");
        var dest = body.Data.Accounts.Single(a => a.AccountName == "Destination");

        Assert.Equal(500m, source.PeriodExpense);
        Assert.Equal(200m, source.PeriodTransferOut);
        Assert.Equal(-700m, source.PeriodNetChange);
        Assert.Equal(200m, dest.PeriodTransferIn);
        Assert.Equal(200m, dest.PeriodNetChange);
    }

    [Fact]
    public async Task GetMonthlyTrend_FillsZeroMonthsAcrossRange()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var foodId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threeMonthsAgo = new DateOnly(today.Year, today.Month, 1).AddMonths(-3);

        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 100m, threeMonthsAgo, foodId);

        var response = await client.GetAsync($"/api/reports/monthly-trend?fromDate={threeMonthsAgo:yyyy-MM-dd}&toDate={today:yyyy-MM-dd}");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<MonthlyTrendReportDto>>();

        // 4 calendar months spanned (3 months ago through current), even though only one has data.
        Assert.Equal(4, body!.Data!.MonthlySeries.Count);
        Assert.Equal(100m, body.Data.MonthlySeries[0].Expense);
        Assert.Equal(0m, body.Data.MonthlySeries[1].Expense);
    }

    [Fact]
    public async Task Reports_DefaultToCurrentMonth_WhenNoDateRangeProvided()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountId = await CreateAccountAsync(client);
        var foodId = await GetFoodExpenseCategoryIdAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-1).AddDays(10);

        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 999m, lastMonth, foodId);
        await SeedTransactionAsync(userId, accountId, TransactionType.Expense, 50m, today, foodId);

        var response = await client.GetAsync("/api/reports/expenses");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IncomeExpenseReportDto>>();

        // Only this month's 50, not last month's 999, since no explicit range was given.
        Assert.Equal(50m, body!.Data!.TotalAmount);
    }

    [Fact]
    public async Task UserA_ReportsNeverIncludeUserBData()
    {
        var (clientB, userBId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var accountB = await CreateAccountAsync(clientB, "User B Bank");
        var foodB = await GetFoodExpenseCategoryIdAsync(clientB);
        await SeedTransactionAsync(userBId, accountB, TransactionType.Expense, 555_555m, DateOnly.FromDateTime(DateTime.UtcNow), foodB);

        var (clientA, _) = await TestAuthHelper.RegisterAndLoginAsync(factory);

        var expenseResponse = await clientA.GetAsync("/api/reports/expenses");
        var expenseBody = await expenseResponse.Content.ReadFromJsonAsync<ApiResponse<IncomeExpenseReportDto>>();

        var accountSummaryResponse = await clientA.GetAsync("/api/reports/account-summary");
        var accountSummaryBody = await accountSummaryResponse.Content.ReadFromJsonAsync<ApiResponse<AccountSummaryReportDto>>();

        Assert.Equal(0m, expenseBody!.Data!.TotalAmount);
        Assert.DoesNotContain(accountSummaryBody!.Data!.Accounts, a => a.AccountName == "User B Bank");
    }

    [Fact]
    public async Task AnonymousUser_CannotAccessReports()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/reports/cash-flow");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
