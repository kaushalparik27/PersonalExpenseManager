using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Application.Accounts.Dtos;
using PersonalExpenseManager.Application.Categories.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.RecurringTransactions;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;
using PersonalExpenseManager.Infrastructure.Persistence;

namespace PersonalExpenseManager.IntegrationTests.Recurring;

// Exercises the recurring transaction engine (RecurringTransactionProcessor) directly
// via DI, bypassing the timer-driven background service (disabled in the Testing
// environment — see Program.cs) so each run is deterministic and test-controlled.
public class RecurringTransactionProcessorTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<(Guid AccountId, Guid CategoryId)> SeedAccountAndExpenseCategoryAsync(HttpClient client)
    {
        var accountResponse = await client.PostAsJsonAsync("/api/accounts",
            new CreateAccountRequest("Test Account", AccountType.Bank, null, 1000m, "INR", null, 0));
        var account = (await accountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>())!.Data!;

        var categoryResponse = await client.PostAsJsonAsync("/api/expense-categories", new CreateCategoryRequest("Rent", null));
        var category = (await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>())!.Data!;

        return (account.Id, category.Id);
    }

    private async Task<RecurringTransaction> SeedRecurringAsync(
        Guid userId, Guid accountId, Guid categoryId, DateOnly nextOccurrenceDate, RecurrenceFrequency frequency,
        DateOnly? endDate = null, bool isActive = true)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var recurring = new RecurringTransaction
        {
            UserId = userId,
            TransactionType = TransactionType.Expense,
            AccountId = accountId,
            ExpenseCategoryId = categoryId,
            Amount = 500m,
            Description = "Rent",
            Frequency = frequency,
            StartDate = nextOccurrenceDate,
            NextOccurrenceDate = nextOccurrenceDate,
            EndDate = endDate,
            IsActive = isActive,
        };

        db.RecurringTransactions.Add(recurring);
        await db.SaveChangesAsync();

        return recurring;
    }

    private async Task<int> RunProcessorAsync()
    {
        using var scope = factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IRecurringTransactionProcessor>();
        return await processor.ProcessDueAsync();
    }

    private async Task<List<Transaction>> GetGeneratedTransactionsAsync(Guid recurringId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Transactions.Where(t => t.RecurringTransactionId == recurringId).OrderBy(t => t.TransactionDate).ToListAsync();
    }

    [Fact]
    public async Task ProcessDueAsync_DueRecurringTransaction_GeneratesTransactionAndAdvancesCursor()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var recurring = await SeedRecurringAsync(userId, accountId, categoryId, today, RecurrenceFrequency.Monthly);

        var generatedCount = await RunProcessorAsync();
        var transactions = await GetGeneratedTransactionsAsync(recurring.Id);

        Assert.Equal(1, generatedCount);
        Assert.Single(transactions);
        Assert.Equal(today, transactions[0].TransactionDate);
        Assert.True(transactions[0].IsRecurring);
        Assert.Equal(500m, transactions[0].Amount);
    }

    [Fact]
    public async Task ProcessDueAsync_NotYetDue_GeneratesNothing()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(2);
        var recurring = await SeedRecurringAsync(userId, accountId, categoryId, futureDate, RecurrenceFrequency.Monthly);

        var generatedCount = await RunProcessorAsync();
        var transactions = await GetGeneratedTransactionsAsync(recurring.Id);

        Assert.Equal(0, generatedCount);
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task ProcessDueAsync_Inactive_GeneratesNothing()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var recurring = await SeedRecurringAsync(userId, accountId, categoryId, today, RecurrenceFrequency.Monthly, isActive: false);

        var generatedCount = await RunProcessorAsync();
        var transactions = await GetGeneratedTransactionsAsync(recurring.Id);

        Assert.Equal(0, generatedCount);
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task ProcessDueAsync_MultipleMissedMonths_CatchesUpAllOfThem()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threeMonthsAgo = today.AddMonths(-3);
        var recurring = await SeedRecurringAsync(userId, accountId, categoryId, threeMonthsAgo, RecurrenceFrequency.Monthly);

        var generatedCount = await RunProcessorAsync();
        var transactions = await GetGeneratedTransactionsAsync(recurring.Id);

        // threeMonthsAgo, -2, -1, and the current month's occurrence: 4 total.
        Assert.Equal(4, generatedCount);
        Assert.Equal(4, transactions.Count);
        Assert.Equal(threeMonthsAgo, transactions[0].TransactionDate);
    }

    [Fact]
    public async Task ProcessDueAsync_RunTwiceInARow_DoesNotDuplicate()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var recurring = await SeedRecurringAsync(userId, accountId, categoryId, today, RecurrenceFrequency.Monthly);

        await RunProcessorAsync();
        var secondRunCount = await RunProcessorAsync();
        var transactions = await GetGeneratedTransactionsAsync(recurring.Id);

        Assert.Equal(0, secondRunCount);
        Assert.Single(transactions);
    }

    [Fact]
    public async Task ProcessDueAsync_PastEndDate_GeneratesFinalOccurrenceThenDeactivates()
    {
        var (client, userId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountId, categoryId) = await SeedAccountAndExpenseCategoryAsync(client);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddMonths(-1);
        var endDate = today.AddDays(-1); // ended before this month's occurrence would fall due
        var recurring = await SeedRecurringAsync(userId, accountId, categoryId, startDate, RecurrenceFrequency.Monthly, endDate: endDate);

        await RunProcessorAsync();
        var transactions = await GetGeneratedTransactionsAsync(recurring.Id);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var refreshed = await db.RecurringTransactions.AsNoTracking().FirstAsync(r => r.Id == recurring.Id);

        Assert.Single(transactions); // only startDate, since the next occurrence would exceed endDate
        Assert.False(refreshed.IsActive);
    }

    [Fact]
    public async Task ProcessDueAsync_ProcessesAllUsersWithoutCrossContamination()
    {
        var (clientA, userAId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountAId, categoryAId) = await SeedAccountAndExpenseCategoryAsync(clientA);
        var (clientB, userBId) = await TestAuthHelper.RegisterAndLoginAsync(factory);
        var (accountBId, categoryBId) = await SeedAccountAndExpenseCategoryAsync(clientB);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var recurringA = await SeedRecurringAsync(userAId, accountAId, categoryAId, today, RecurrenceFrequency.Monthly);
        var recurringB = await SeedRecurringAsync(userBId, accountBId, categoryBId, today, RecurrenceFrequency.Monthly);

        var generatedCount = await RunProcessorAsync();

        var transactionsA = await GetGeneratedTransactionsAsync(recurringA.Id);
        var transactionsB = await GetGeneratedTransactionsAsync(recurringB.Id);

        Assert.Equal(2, generatedCount);
        Assert.Single(transactionsA);
        Assert.Single(transactionsB);
        Assert.Equal(userAId, transactionsA[0].UserId);
        Assert.Equal(userBId, transactionsB[0].UserId);
    }
}
