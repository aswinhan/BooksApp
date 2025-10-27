using Modules.Common.Domain;
using Modules.Catalog.Domain.ValueObjects; // Required for Rating
using System;
using System.Collections.Generic;
using System.Linq; // Required for LINQ methods like FirstOrDefault, Any

namespace Modules.Catalog.Domain.Entities;

public class Book : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Isbn { get; private set; } = null!; // International Standard Book Number
    public decimal Price { get; private set; }
    public string? CoverImageUrl { get; private set; }

    public Guid AuthorId { get; private set; } // Foreign key to Author
    public Author Author { get; private set; } = null!; // Navigation property

    // --- ADD Category Relationship ---
    public Guid? CategoryId { get; private set; } // Foreign key to Category
    public Category? Category { get; private set; } = null!; // Navigation property

    // Use a private field to hold reviews, expose as read-only
    private readonly List<Review> _reviews = [];
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    // Private constructor for EF Core
    private Book() { }

    // Public constructor for creating a new Book
    public Book(Guid id, string title, string? description, string isbn, decimal price, Guid authorId, Guid? categoryId, string? coverImageUrl = null)
    {
        // Basic validation
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN cannot be empty.", nameof(isbn));


        Id = id;
        Title = title;
        Description = description;
        Isbn = isbn;
        Price = price;
        AuthorId = authorId;
        CategoryId = categoryId;
        CoverImageUrl = coverImageUrl;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- Business Logic within the Aggregate Root ---

    /// <summary>
    /// Adds a review to the book, enforcing business rules.
    /// </summary>
    public void AddReview(string userId, string comment, Rating rating)
    {
        // Rule: Check if user has already reviewed this book
        if (_reviews.Any(r => r.UserId == userId))
        {
            // Domain Exception or Result<Error> could be used here too
            throw new InvalidOperationException($"User {userId} has already reviewed this book.");
        }

        var review = new Review(Guid.NewGuid(), this.Id, userId, comment, rating);
        _reviews.Add(review);

        UpdatedAtUtc = DateTime.UtcNow; // Mark book as updated
    }

    /// <summary>
    /// Updates the book's core details.
    /// </summary>
    public void UpdateDetails(string title, string? description, string isbn, decimal price, Guid authorId, Guid? categoryId, string? coverImageUrl)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN cannot be empty.", nameof(isbn));

        Title = title;
        Description = description;
        Isbn = isbn;
        Price = price;
        AuthorId = authorId; // Assuming Author can be changed
        CategoryId = categoryId;
        CoverImageUrl = coverImageUrl;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // Method specifically for updating the cover image URL
    public void SetCoverImage(string? imageUrl)
    {
        CoverImageUrl = imageUrl;
        UpdatedAtUtc = DateTime.UtcNow;
    }


    // IAuditableEntity properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}