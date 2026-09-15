using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalExpenseManager.Domain.Entities;
using PersonalExpenseManager.Infrastructure.Identity;

namespace PersonalExpenseManager.Infrastructure.Persistence.Configurations;

public class ApplicationSettingsConfiguration : IEntityTypeConfiguration<ApplicationSettings>
{
    public void Configure(EntityTypeBuilder<ApplicationSettings> builder)
    {
        builder.ToTable("ApplicationSettings");

        builder.HasKey(s => s.UserId);

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<ApplicationSettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.DefaultCurrency).IsRequired().HasMaxLength(3);
    }
}
