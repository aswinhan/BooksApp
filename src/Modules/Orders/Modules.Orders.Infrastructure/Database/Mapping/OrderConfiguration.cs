using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Orders.Domain.Entities;
using Modules.Orders.Domain.Enums; // Required for OrderStatus

namespace Modules.Orders.Infrastructure.Database.Mapping;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId).IsRequired();

        // Configure Shipping Address
        builder.OwnsOne(o => o.ShippingAddress, addressBuilder =>
        {
            // Explicitly map column names with prefix if using snake case naming convention
            addressBuilder.Property(a => a.Street).HasColumnName("shipping_street").IsRequired().HasMaxLength(200);
            addressBuilder.Property(a => a.City).HasColumnName("shipping_city").IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.State).HasColumnName("shipping_state").IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.ZipCode).HasColumnName("shipping_zip_code").IsRequired().HasMaxLength(20);
        });

        // --- ADD Billing Address Configuration ---
        builder.OwnsOne(o => o.BillingAddress, addressBuilder =>
        {
            // Use a different prefix for billing address columns
            addressBuilder.Property(a => a.Street).HasColumnName("billing_street").IsRequired().HasMaxLength(200);
            addressBuilder.Property(a => a.City).HasColumnName("billing_city").IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.State).HasColumnName("billing_state").IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.ZipCode).HasColumnName("billing_zip_code").IsRequired().HasMaxLength(20);
        });
        // --- End Billing Address ---

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>() // Store enum as string
            .HasMaxLength(50);

        builder.Property(o => o.Total)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(o => o.PaymentMethod)
       .IsRequired()
       .HasConversion<string>() // Store enum as string
       .HasMaxLength(50);

        builder.Property(o => o.PaymentIntentId)
       .HasMaxLength(100); // Allow reasonable length for Stripe IDs

        builder.Property(o => o.TaxAmount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();
        builder.Property(o => o.ShippingCost)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        // Configure the one-to-many relationship with OrderItem
        // Map the private '_orderItems' field
        builder.HasMany(o => o.OrderItems) // Order has many OrderItems
               .WithOne(oi => oi.Order) // OrderItem has one Order
               .HasForeignKey(oi => oi.OrderId) // Foreign key in OrderItem
               .IsRequired();
        builder.Navigation(o => o.OrderItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}