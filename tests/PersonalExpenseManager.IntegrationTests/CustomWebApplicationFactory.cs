using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Infrastructure.Persistence;

namespace PersonalExpenseManager.IntegrationTests;

// Points the API at a dedicated test database on the same local Postgres instance
// used for development (see docker-compose.yml). Schema is recreated fresh for
// every test class; xunit collection parallelization is disabled (see
// AssemblyInfo.cs) so test classes never race against the same database.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestConnectionString =
        "Host=localhost;Port=5432;Database=personal_expense_manager_test;Username=pem_user;Password=pem_dev_password";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
                ["Frontend:BaseUrl"] = "http://localhost:5173",
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureDeletedAsync();
    }
}
