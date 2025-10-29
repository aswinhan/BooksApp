using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Wishlist.Domain.Entities;

namespace Modules.Wishlist.Infrastructure.Database.Mapping;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(wi => wi.Id);

        builder.Property(wi => wi.UserId).IsRequired();
        builder.Property(wi => wi.BookId).IsRequired();

        // Create a unique index on UserId and BookId to prevent duplicates
        builder.HasIndex(wi => new { wi.UserId, wi.BookId }).IsUnique();
    }
}