using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Configurations;

public class IncomeCategoryConfiguration : IEntityTypeConfiguration<IncomeCategory>
{
    public void Configure(EntityTypeBuilder<IncomeCategory> builder)
    {
        builder.ToTable("IncomeCategories");

        builder.HasKey(c => c.Id);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(50);

        builder.HasMany(c => c.SubCategories)
            .WithOne(s => s.IncomeCategory)
            .HasForeignKey(s => s.IncomeCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
        builder.HasIndex(c => c.DisplayOrder);
    }
}
