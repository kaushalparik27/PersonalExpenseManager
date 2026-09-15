using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Configurations;

public class IncomeSubCategoryConfiguration : IEntityTypeConfiguration<IncomeSubCategory>
{
    public void Configure(EntityTypeBuilder<IncomeSubCategory> builder)
    {
        builder.ToTable("IncomeSubCategories");

        builder.HasKey(s => s.Id);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.IncomeCategoryId);
        builder.HasIndex(s => new { s.IncomeCategoryId, s.Name }).IsUnique();
    }
}
