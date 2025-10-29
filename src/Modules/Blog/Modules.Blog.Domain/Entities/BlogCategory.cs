using Modules.Common.Domain;
using System;
using System.Collections.Generic;

namespace Modules.Blog.Domain.Entities;

// Category for Blog Posts (e.g., "Tutorials", "News")
public class BlogCategory : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    // Navigation property to Posts (Many-to-Many)
    public ICollection<Post> Posts { get; private set; } = [];

    private BlogCategory() { } // EF Core

    public BlogCategory(Guid id, string name, string slug)
    {
        Id = id;
        Name = name;
        Slug = slug;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Update(string name, string slug)
    {
        Name = name;
        Slug = slug;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}