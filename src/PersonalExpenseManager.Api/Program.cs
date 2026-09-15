using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using PersonalExpenseManager.Api.Filters;
using PersonalExpenseManager.Api.Middleware;
using PersonalExpenseManager.Application;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Infrastructure;
using PersonalExpenseManager.Infrastructure.Persistence.Seeding;
using PersonalExpenseManager.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
    .AddJsonOptions(options =>
    {
        // Enums travel as their names (e.g. "Bank", "Transfer") over the wire,
        // matching how they're stored in Postgres (HasConversion<string>()).
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Integration tests run in the Testing environment and drive the recurring
// transaction engine explicitly via IRecurringTransactionProcessor — the timer-driven
// background sweep would otherwise run unpredictably mid-test.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<RecurringTransactionProcessingService>();
}

// Secure by default: every endpoint requires authentication unless explicitly
// marked [AllowAnonymous] (register/login/forgot-password/reset-password).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Throttles the anonymous auth surface (register/login/forgot-password/reset-password)
// against brute-force and enumeration scripts. A single shared bucket (not partitioned
// per-client) is a deliberate MVP-scale simplification — revisit with per-IP partitioning
// if this ever sits behind a load balancer with real abuse traffic. Testing gets an
// effectively unlimited bucket so xunit test classes sharing one WebApplicationFactory
// (and therefore one limiter instance) never fail on request volume, not attacker behavior.
var isTestingEnvironment = builder.Environment.IsEnvironment("Testing");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = isTestingEnvironment ? 100_000 : 20;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail("Too many requests. Please try again later."),
            cancellationToken);
    };
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await DevDataSeeder.SeedAsync(app.Services);
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
