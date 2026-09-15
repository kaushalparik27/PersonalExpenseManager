using System.Net;
using System.Net.Http.Json;
using PersonalExpenseManager.Application.Auth.Dtos;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.IntegrationTests.Auth;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static RegisterRequest NewRegisterRequest(string? email = null) => new(
        FirstName: "Ada",
        LastName: "Lovelace",
        Email: email ?? $"{Guid.NewGuid():N}@example.com",
        Password: "Str0ng!Passw0rd",
        ConfirmPassword: "Str0ng!Passw0rd");

    [Fact]
    public async Task Register_ValidData_Succeeds()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", NewRegisterRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.NotEqual(Guid.Empty, body.Data!.Id);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Rejected()
    {
        var client = factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.com";

        await client.PostAsJsonAsync("/api/auth/register", NewRegisterRequest(email));
        var second = await client.PostAsJsonAsync("/api/auth/register", NewRegisterRequest(email));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_Rejected()
    {
        var client = factory.CreateClient();
        var request = new RegisterRequest("Ada", "Lovelace", $"{Guid.NewGuid():N}@example.com", "weak", "weak");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Register_PasswordMismatch_Rejected()
    {
        var client = factory.CreateClient();
        var request = new RegisterRequest("Ada", "Lovelace", $"{Guid.NewGuid():N}@example.com", "Str0ng!Passw0rd", "Different1!");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Succeeds()
    {
        var client = factory.CreateClient();
        var register = NewRegisterRequest();
        await client.PostAsJsonAsync("/api/auth/register", register);
        await client.PostAsJsonAsync("/api/auth/logout", (object?)null);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(register.Email, register.Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Fails()
    {
        var client = factory.CreateClient();
        var register = NewRegisterRequest();
        await client.PostAsJsonAsync("/api/auth/register", register);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(register.Email, "WrongPassword1!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Fails()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest($"{Guid.NewGuid():N}@example.com", "Str0ng!Passw0rd"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WhenAuthenticated_ReturnsProfile()
    {
        var client = factory.CreateClient();
        var register = NewRegisterRequest();
        await client.PostAsJsonAsync("/api/auth/register", register);

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        Assert.Equal(register.Email, body!.Data!.Email);
    }

    [Fact]
    public async Task Me_WhenAnonymous_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ThenMe_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", NewRegisterRequest());

        await client.PostAsJsonAsync("/api/auth/logout", (object?)null);
        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_StillReturnsGenericSuccess()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new ForgotPasswordRequest($"{Guid.NewGuid():N}@example.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
