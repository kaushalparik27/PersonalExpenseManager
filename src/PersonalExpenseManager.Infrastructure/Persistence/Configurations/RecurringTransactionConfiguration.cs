using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Configurations;

public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
{
    public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
    {
        builder.ToTable("RecurringTransactions", t =>
            t.HasCheckConstraint("CK_RecurringTransactions_Amount_Positive", "\"Amount\" > 0"));

        builder.HasKey(r => r.Id);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(r => r.TransferToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExpenseCategory>()
            .WithMany()
            .HasForeignKey(r => r.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExpenseSubCategory>()
            .WithMany()
            .HasForeignKey(r => r.ExpenseSubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncomeCategory>()
            .WithMany()
            .HasForeignKey(r => r.IncomeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IncomeSubCategory>()
            .WithMany()
            .HasForeignKey(r => r.IncomeSubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Amount).HasColumnType("NUMERIC(18,2)").IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.TransactionType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Frequency).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.NextOccurrenceDate);
        builder.HasIndex(r => r.IsActive);
    }
}
