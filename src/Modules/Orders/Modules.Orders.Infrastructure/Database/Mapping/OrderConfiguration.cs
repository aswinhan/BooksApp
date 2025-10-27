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

        // Configure the owned Address Value Object
        builder.OwnsOne(o => o.ShippingAddress, addressBuilder =>
        {
            // Map properties of Address to columns in the Orders table
            // Naming convention will handle snake_case (e.g., shipping_address_street)
            addressBuilder.Property(a => a.Street).IsRequired().HasMaxLength(200);
            addressBuilder.Property(a => a.City).IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.State).IsRequired().HasMaxLength(100);
            addressBuilder.Property(a => a.ZipCode).IsRequired().HasMaxLength(20);
        });

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