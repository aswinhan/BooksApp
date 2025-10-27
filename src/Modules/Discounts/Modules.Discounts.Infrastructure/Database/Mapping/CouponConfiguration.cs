using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Discounts.Domain.Entities;
using Modules.Discounts.Domain.Enums; // Required for DiscountType

namespace Modules.Discounts.Infrastructure.Database.Mapping;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
               .IsRequired()
               .HasMaxLength(50); // Max length for coupon code
        builder.HasIndex(c => c.Code).IsUnique(); // Ensure codes are unique

        builder.Property(c => c.Type)
               .IsRequired()
               .HasConversion<string>() // Store enum as string
               .HasMaxLength(50);

        builder.Property(c => c.Value)
               .HasColumnType("decimal(18,2)") // Precision for value
               .IsRequired();

        builder.Property(c => c.ExpiryDate); // Nullable

        builder.Property(c => c.UsageLimit).IsRequired();
        builder.Property(c => c.UsageCount).IsRequired();

        builder.Property(c => c.MinimumCartAmount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(c => c.IsActive).IsRequired();
    }
}