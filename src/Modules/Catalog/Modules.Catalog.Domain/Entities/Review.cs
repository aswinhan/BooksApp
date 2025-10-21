using Modules.Common.Domain;
using Modules.Catalog.Domain.ValueObjects; // Required for Rating
using System;

namespace Modules.Catalog.Domain.Entities;

public class Review : IAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid BookId { get; private set; } // Foreign key to Book
    public string UserId { get; private set; } = null!; // Foreign key to User (using string ID)
    public string Comment { get; private set; } = null!;
    public Rating Rating { get; private set; } = null!; // Embed the Rating Value Object

    // Navigation property back to Book (optional but useful for EF Core)
    public Book Book { get; private set; } = null!;

    // Private constructor for EF Core
    private Review() { }

    // Internal constructor: Only the Book aggregate should create Reviews
    internal Review(Guid id, Guid bookId, string userId, string comment, Rating rating)
    {
        Id = id;
        BookId = bookId;
        UserId = userId;
        Comment = comment;
        Rating = rating;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // IAuditableEntity properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}