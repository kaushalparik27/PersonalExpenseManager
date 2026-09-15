using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Dashboard;
using PersonalExpenseManager.Application.Transactions;
using PersonalExpenseManager.Infrastructure.Email;
using PersonalExpenseManager.Infrastructure.Identity;
using PersonalExpenseManager.Infrastructure.Persistence;
using PersonalExpenseManager.Infrastructure.Persistence.Commands;
using PersonalExpenseManager.Infrastructure.Persistence.Queries;
using PersonalExpenseManager.Infrastructure.Persistence.Seeding;

namespace PersonalExpenseManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddScoped<IEmailSender, DevEmailSender>();
        services.AddScoped<IAccountBalanceCalculator, AccountBalanceCalculator>();
        services.AddScoped<IAccountQueries, AccountQueries>();
        services.AddScoped<IAccountCommands, AccountCommands>();
        services.AddScoped<IDefaultCategorySeeder, DefaultCategorySeeder>();
        services.AddScoped<IExpenseCategoryQueries, ExpenseCategoryQueries>();
        services.AddScoped<IExpenseCategoryCommands, ExpenseCategoryCommands>();
        services.AddScoped<IIncomeCategoryQueries, IncomeCategoryQueries>();
        services.AddScoped<IIncomeCategoryCommands, IncomeCategoryCommands>();
        services.AddScoped<ITransactionQueries, TransactionQueries>();
        services.AddScoped<ITransactionCommands, TransactionCommands>();
        services.AddScoped<IDashboardQueries, DashboardQueries>();

        return services;
    }
}
