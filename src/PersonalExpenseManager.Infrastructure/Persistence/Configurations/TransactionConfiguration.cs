using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", t =>
        {
            // Adjustments may be signed (a correction can go either direction);
            // every other transaction type stores an unsigned magnitude.
            t.HasCheckConstraint(
                "CK_Transactions_Amount_Sign",
                "(\"TransactionType\" = 'Adjustment' AND \"Amount\" <> 0) OR (\"TransactionType\" <> 'Adjustment' AND \"Amount\" > 0)");
            t.HasCheckConstraint(
                "CK_Transactions_Transfer_DifferentAccounts",
                "\"TransferToAccountId\" IS NULL OR \"TransferToAccountId\" <> \"AccountId\"");
        });

        builder.HasKey(t => t.Id);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(t => t.TransferToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExpenseCategory>()
            .WithMany()
            .HasForeignKey(t => t.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExpenseSubCategory>()
            .WithMany()
            .HasForeignKey(t => t.ExpenseSubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncomeCategory>()
            .WithMany()
            .HasForeignKey(t => t.IncomeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncomeSubCategory>()
            .WithMany()
            .HasForeignKey(t => t.IncomeSubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RecurringTransaction>()
            .WithMany()
            .HasForeignKey(t => t.RecurringTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(t => t.Amount).HasColumnType("NUMERIC(18,2)").IsRequired();
        builder.Property(t => t.TransactionType).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.Notes).HasMaxLength(2000);
        builder.Property(t => t.Payee).HasMaxLength(200);
        builder.Property(t => t.ReferenceNumber).HasMaxLength(100);

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.TransactionType);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.ExpenseCategoryId);
        builder.HasIndex(t => t.ExpenseSubCategoryId);
        builder.HasIndex(t => t.IncomeCategoryId);
        builder.HasIndex(t => t.IncomeSubCategoryId);
        builder.HasIndex(t => new { t.UserId, t.TransactionDate });
    }
}
