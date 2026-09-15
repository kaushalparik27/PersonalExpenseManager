using System.Net.Http.Json;
using PersonalExpenseManager.Application.Auth.Dtos;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.IntegrationTests;

internal static class TestAuthHelper
{
    public static async Task<(HttpClient Client, Guid UserId)> RegisterAndLoginAsync(
        CustomWebApplicationFactory factory,
        string? email = null)
    {
        var client = factory.CreateClient();
        var request = new RegisterRequest("Test", "User", email ?? $"{Guid.NewGuid():N}@example.com", "Str0ng!Passw0rd", "Str0ng!Passw0rd");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();

        return (client, body!.Data!.Id);
    }
}
