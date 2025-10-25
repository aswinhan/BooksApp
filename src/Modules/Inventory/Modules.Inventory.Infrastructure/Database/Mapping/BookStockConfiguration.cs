using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Inventory.Domain.Entities;

namespace Modules.Inventory.Infrastructure.Database.Mapping;

public class BookStockConfiguration : IEntityTypeConfiguration<BookStock>
{
    public void Configure(EntityTypeBuilder<BookStock> builder)
    {
        builder.HasKey(bs => bs.Id);

        builder.Property(bs => bs.BookId).IsRequired();
        builder.HasIndex(bs => bs.BookId).IsUnique(); // One stock record per book

        builder.Property(bs => bs.QuantityAvailable).IsRequired();
    }
}