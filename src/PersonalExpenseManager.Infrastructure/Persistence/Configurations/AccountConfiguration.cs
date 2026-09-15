using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.AccountType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.OpeningBalance)
            .HasColumnType("NUMERIC(18,2)")
            .IsRequired();

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(a => a.Icon)
            .HasMaxLength(50);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.IsActive);
        builder.HasIndex(a => a.DisplayOrder);
    }
}
