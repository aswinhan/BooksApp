using Modules.Common.Domain; // Required for IAuditableEntity
using System;
using System.Collections.Generic;

namespace Modules.Catalog.Domain.Entities;

public class Author : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Biography { get; private set; } // Biography can be optional

    // Navigation property to Books (one author -> many books)
    public ICollection<Book> Books { get; private set; } = [];

    // Private constructor for EF Core
    private Author() { }

    // Public constructor for creating a new Author
    public Author(Guid id, string name, string? biography)
    {
        Id = id;
        Name = name; // Basic validation can be added here if needed
        Biography = biography;
        CreatedAtUtc = DateTime.UtcNow; // Set audit field
    }

    // Method to update author details (optional)
    public void UpdateDetails(string name, string? biography)
    {
        Name = name;
        Biography = biography;
        UpdatedAtUtc = DateTime.UtcNow; // Set audit field
    }

    // IAuditableEntity properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}