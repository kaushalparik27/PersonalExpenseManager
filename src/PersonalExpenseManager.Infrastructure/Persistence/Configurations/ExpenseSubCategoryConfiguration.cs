using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Configurations;

public class ExpenseSubCategoryConfiguration : IEntityTypeConfiguration<ExpenseSubCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseSubCategory> builder)
    {
        builder.ToTable("ExpenseSubCategories");

        builder.HasKey(s => s.Id);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Icon).HasMaxLength(50);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ExpenseCategoryId);
        builder.HasIndex(s => new { s.ExpenseCategoryId, s.Name }).IsUnique();
    }
}
