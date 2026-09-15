using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Application.Auth.Dtos;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;
using PersonalExpenseManager.Infrastructure.Persistence;

namespace PersonalExpenseManager.IntegrationTests.Auth;

// Proves the ownership-isolation rule the Authentication phase must satisfy before
// any financial feature work proceeds: a resource scoped to one user must be
// completely inaccessible to another authenticated user.
public class CrossUserAccessTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<(HttpClient Client, Guid UserId)> RegisterAndLoginAsync()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest("Test", "User", email, "Str0ng!Passw0rd", "Str0ng!Passw0rd");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();

        return (client, body!.Data!.Id);
    }

    private async Task<Guid> SeedAccountAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var account = new Account
        {
            UserId = userId,
            Name = "Private Account",
            AccountType = AccountType.Bank,
            OpeningBalance = 1000m,
            Currency = "INR",
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        return account.Id;
    }

    [Fact]
    public async Task AnonymousUser_CannotListAccounts()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousUser_CannotReadSpecificAccount()
    {
        var (_, userId) = await RegisterAndLoginAsync();
        var accountId = await SeedAccountAsync(userId);

        var anonymousClient = factory.CreateClient();
        var response = await anonymousClient.GetAsync($"/api/accounts/{accountId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUser_CanReadOwnAccount()
    {
        var (client, userId) = await RegisterAndLoginAsync();
        var accountId = await SeedAccountAsync(userId);

        var response = await client.GetAsync($"/api/accounts/{accountId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UserA_CannotReadUserBAccount()
    {
        var (_, userBId) = await RegisterAndLoginAsync();
        var userBAccountId = await SeedAccountAsync(userBId);

        var (userAClient, _) = await RegisterAndLoginAsync();
        var response = await userAClient.GetAsync($"/api/accounts/{userBAccountId}");

        // 404, not 403: existence of another user's resource must not be revealed.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserA_ListingAccounts_NeverIncludesUserBAccounts()
    {
        var (_, userBId) = await RegisterAndLoginAsync();
        await SeedAccountAsync(userBId);
        await SeedAccountAsync(userBId);

        var (userAClient, userAId) = await RegisterAndLoginAsync();
        await SeedAccountAsync(userAId);

        var response = await userAClient.GetAsync("/api/accounts");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<AccountListItem>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(body!.Data!);
    }

    private record AccountListItem(Guid Id, string Name);
}
