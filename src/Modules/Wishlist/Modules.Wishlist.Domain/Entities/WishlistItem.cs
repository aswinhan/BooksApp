using Modules.Common.Domain;
using System;

namespace Modules.Wishlist.Domain.Entities;

// Represents a single book in a user's wishlist
public class WishlistItem : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!; // Foreign key to User (string)
    public Guid BookId { get; private set; } // Foreign key to Catalog.Book (Guid)

    private WishlistItem() { } // EF Core

    public WishlistItem(Guid id, string userId, Guid bookId)
    {
        Id = id;
        UserId = userId;
        BookId = bookId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; } // Not typically updated, but keep for interface
}