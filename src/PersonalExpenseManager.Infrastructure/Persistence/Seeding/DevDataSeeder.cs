using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Domain.Enums;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Seeding;

// Development-only realistic seed data: a demo user with accounts, categories,
// and several months of transactions, so the dashboard/reports UI has something
// to render before real usage accumulates. Never runs outside Development, and
// is idempotent (no-ops once the demo user already exists). Not permanent system
// data - the demo user is a fixture like any other user account.
public static class DevDataSeeder
{
    public const string DemoEmail = "demo@example.com";
    public const string DemoPassword = "Demo!Passw0rd";

    // Icon values must match a key in the frontend's CATEGORY_ICON_ENTRIES
    // (client/src/lib/categoryIcons.ts) — kept in sync manually since the icon
    // registry lives client-side and the backend just stores an opaque string.
    private static readonly (string Name, string Icon)[] ExpenseCategories =
    [
        ("Food", "Utensils"),
        ("Transportation", "Car"),
        ("Utilities", "Lightbulb"),
        ("Home", "Home"),
        ("Healthcare", "HeartPulse"),
        ("Entertainment", "Clapperboard"),
        ("Shopping", "ShoppingBag"),
        ("Education", "GraduationCap"),
        ("Insurance", "Shield"),
        ("Investment", "TrendingUp"),
        ("Other", "Tag"),
    ];

    private static readonly (string Name, string Icon)[] IncomeCategories =
    [
        ("Salary", "Landmark"),
        ("Freelance", "HandCoins"),
        ("Interest", "Percent"),
        ("Other Income", "Tag"),
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await userManager.FindByEmailAsync(DemoEmail) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = DemoEmail,
            Email = DemoEmail,
            EmailConfirmed = true,
            FirstName = "Demo",
            LastName = "User",
        };

        var createResult = await userManager.CreateAsync(user, DemoPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to create demo user: " + string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        var bank = new Account { UserId = user.Id, Name = "HDFC Bank", AccountType = AccountType.Bank, OpeningBalance = 150_000m, DisplayOrder = 0 };
        var sbi = new Account { UserId = user.Id, Name = "SBI Bank", AccountType = AccountType.Bank, OpeningBalance = 75_000m, DisplayOrder = 1 };
        var cash = new Account { UserId = user.Id, Name = "Home Cash", AccountType = AccountType.Cash, OpeningBalance = 5_000m, DisplayOrder = 2 };
        var creditCard = new Account { UserId = user.Id, Name = "HDFC Credit Card", AccountType = AccountType.CreditCard, OpeningBalance = 0m, DisplayOrder = 3 };
        db.Accounts.AddRange(bank, sbi, cash, creditCard);

        var expenseCategories = ExpenseCategories
            .Select((c, index) => new ExpenseCategory { UserId = user.Id, Name = c.Name, Icon = c.Icon, DisplayOrder = index })
            .ToArray();
        db.ExpenseCategories.AddRange(expenseCategories);

        var food = expenseCategories.Single(c => c.Name == "Food");
        var transportation = expenseCategories.Single(c => c.Name == "Transportation");
        var utilities = expenseCategories.Single(c => c.Name == "Utilities");

        ExpenseSubCategory Sub(ExpenseCategory category, string name, int order) =>
            new() { UserId = user.Id, ExpenseCategoryId = category.Id, Name = name, DisplayOrder = order };

        var groceries = Sub(food, "Groceries", 0);
        var restaurants = Sub(food, "Restaurants", 1);
        var delivery = Sub(food, "Delivery", 2);
        var fuel = Sub(transportation, "Fuel", 0);
        var taxi = Sub(transportation, "Taxi", 1);
        var electricity = Sub(utilities, "Electricity", 0);
        var internet = Sub(utilities, "Internet", 1);
        var mobile = Sub(utilities, "Mobile", 2);
        db.ExpenseSubCategories.AddRange(groceries, restaurants, delivery, fuel, taxi, electricity, internet, mobile);

        var incomeCategories = IncomeCategories
            .Select((c, index) => new IncomeCategory { UserId = user.Id, Name = c.Name, Icon = c.Icon, DisplayOrder = index })
            .ToArray();
        db.IncomeCategories.AddRange(incomeCategories);

        var salary = incomeCategories.Single(c => c.Name == "Salary");
        var freelance = incomeCategories.Single(c => c.Name == "Freelance");
        var interest = incomeCategories.Single(c => c.Name == "Interest");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodStart = today.AddMonths(-4);
        var random = new Random(20260915); // fixed seed: identical demo data on every fresh seed run

        var transactions = new List<Transaction>();

        for (var month = 0; month < 4; month++)
        {
            var salaryDate = new DateOnly(periodStart.Year, periodStart.Month, 1).AddMonths(month);
            transactions.Add(new Transaction
            {
                UserId = user.Id,
                TransactionDate = salaryDate,
                TransactionType = TransactionType.Income,
                AccountId = bank.Id,
                Amount = 85_000m,
                IncomeCategoryId = salary.Id,
                Description = "Monthly salary",
            });
        }

        (ExpenseSubCategory Sub, string[] Descriptions, decimal Min, decimal Max)[] expenseSamples =
        [
            (groceries, ["Grocery shopping", "Supermarket run", "Vegetables"], 300m, 2500m),
            (restaurants, ["Dinner out", "Lunch with friends", "Cafe"], 200m, 1800m),
            (delivery, ["Food delivery", "Online order"], 150m, 900m),
            (fuel, ["Fuel refill", "Petrol"], 500m, 3000m),
            (taxi, ["Cab ride", "Auto fare"], 100m, 600m),
            (electricity, ["Electricity bill"], 800m, 2500m),
            (internet, ["Internet bill"], 700m, 1200m),
            (mobile, ["Mobile recharge"], 200m, 500m),
        ];

        var totalDays = today.DayNumber - periodStart.DayNumber;

        for (var i = 0; i < 90; i++)
        {
            var date = periodStart.AddDays(random.Next(0, totalDays));
            var sample = expenseSamples[random.Next(expenseSamples.Length)];
            var amount = Math.Round(sample.Min + (decimal)random.NextDouble() * (sample.Max - sample.Min), MidpointRounding.AwayFromZero);
            var account = random.Next(2) == 0 ? bank : creditCard;

            transactions.Add(new Transaction
            {
                UserId = user.Id,
                TransactionDate = date,
                TransactionType = TransactionType.Expense,
                AccountId = account.Id,
                Amount = amount,
                ExpenseCategoryId = sample.Sub.ExpenseCategoryId,
                ExpenseSubCategoryId = sample.Sub.Id,
                Description = sample.Descriptions[random.Next(sample.Descriptions.Length)],
            });
        }

        transactions.Add(new Transaction
        {
            UserId = user.Id,
            TransactionDate = periodStart.AddDays(10),
            TransactionType = TransactionType.Income,
            AccountId = sbi.Id,
            Amount = 18_000m,
            IncomeCategoryId = freelance.Id,
            Description = "Freelance project payment",
        });
        transactions.Add(new Transaction
        {
            UserId = user.Id,
            TransactionDate = periodStart.AddDays(70),
            TransactionType = TransactionType.Income,
            AccountId = sbi.Id,
            Amount = 12_500m,
            IncomeCategoryId = freelance.Id,
            Description = "Freelance project payment",
        });
        transactions.Add(new Transaction
        {
            UserId = user.Id,
            TransactionDate = periodStart.AddDays(45),
            TransactionType = TransactionType.Income,
            AccountId = sbi.Id,
            Amount = 1_250m,
            IncomeCategoryId = interest.Id,
            Description = "Savings account interest",
        });

        transactions.Add(new Transaction
        {
            UserId = user.Id,
            TransactionDate = today.AddDays(-20),
            TransactionType = TransactionType.Transfer,
            AccountId = bank.Id,
            TransferToAccountId = cash.Id,
            Amount = 5_000m,
            Description = "Cash withdrawal",
        });
        transactions.Add(new Transaction
        {
            UserId = user.Id,
            TransactionDate = today.AddDays(-50),
            TransactionType = TransactionType.Transfer,
            AccountId = bank.Id,
            TransferToAccountId = sbi.Id,
            Amount = 20_000m,
            Description = "Move funds to savings",
        });

        db.Transactions.AddRange(transactions);

        await db.SaveChangesAsync(cancellationToken);
    }
}
