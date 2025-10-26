using Modules.Common.Domain;
using System;
using System.Collections.Generic;

namespace Modules.Catalog.Domain.Entities;

public class Category : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!; // URL-friendly identifier
    // Optional: Parent category for subcategories
    // public Guid? ParentCategoryId { get; private set; }
    // public Category? ParentCategory { get; private set; }
    // public ICollection<Category> Subcategories { get; private set; } = new List<Category>();

    // Navigation property to Books (many-to-many would be better, but start simple)
    // For simplicity, let's assume a Book belongs to ONE Category for now.
    public ICollection<Book> Books { get; private set; } = new List<Book>();


    private Category() { } // For EF Core

    public Category(Guid id, string name, string slug)
    {
        // Add validation
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty.", nameof(slug));

        Id = id;
        Name = name;
        Slug = slug; // Ensure uniqueness
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Update(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty.", nameof(slug));
        Name = name;
        Slug = slug;
        UpdatedAtUtc = DateTime.UtcNow;
    }


    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}