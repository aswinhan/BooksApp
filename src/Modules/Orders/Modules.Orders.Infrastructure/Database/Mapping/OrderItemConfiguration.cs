using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Orders.Domain.Entities;

namespace Modules.Orders.Infrastructure.Database.Mapping;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.BookId).IsRequired();

        builder.Property(oi => oi.BookTitle)
            .IsRequired()
            .HasMaxLength(200); // Match Book Title length?

        builder.Property(oi => oi.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(oi => oi.Quantity).IsRequired();

        // Foreign key configured in OrderConfiguration
    }
}