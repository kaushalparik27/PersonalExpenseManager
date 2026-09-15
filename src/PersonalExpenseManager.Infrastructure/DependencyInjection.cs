using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Application.Accounts;
using PersonalExpenseManager.Application.Categories;
using PersonalExpenseManager.Application.Common;
using PersonalExpenseManager.Application.Dashboard;
using PersonalExpenseManager.Application.RecurringTransactions;
using PersonalExpenseManager.Application.Reports;
using PersonalExpenseManager.Application.Transactions;
using PersonalExpenseManager.Infrastructure.Email;
using PersonalExpenseManager.Infrastructure.Identity;
using PersonalExpenseManager.Infrastructure.Persistence;
using PersonalExpenseManager.Infrastructure.Persistence.Commands;
using PersonalExpenseManager.Infrastructure.Persistence.Queries;
using PersonalExpenseManager.Infrastructure.Persistence.Seeding;
using PersonalExpenseManager.Infrastructure.Services;

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

        // CSRF: no anti-forgery token is issued, by design. The auth cookie is
        // HttpOnly (JS can't read/exfiltrate it) and SameSite=Strict, which stops
        // the browser from attaching it to any cross-site request — including the
        // classic CSRF pattern of a form/fetch on an attacker-controlled page
        // targeting this API. That covers every state-changing endpoint here since
        // all of them require the cookie. Same-origin requests from the SPA itself
        // (or another localhost port in dev, which shares the same registrable
        // domain) still carry it normally. Don't remove SameSite=Strict without
        // adding an explicit anti-forgery token scheme first — HTTPS alone does not
        // stand in for this.
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
        services.AddScoped<IReportsQueries, ReportsQueries>();
        services.AddScoped<IRecurringTransactionQueries, RecurringTransactionQueries>();
        services.AddScoped<IRecurringTransactionCommands, RecurringTransactionCommands>();
        services.AddScoped<IRecurringTransactionProcessor, RecurringTransactionProcessor>();

        return services;
    }
}
