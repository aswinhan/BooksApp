using Modules.Common.Domain;
using System;
using System.Collections.Generic;

namespace Modules.Blog.Domain.Entities;

// Tag for Blog Posts (e.g., ".NET", "Architecture")
public class Tag : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    // Navigation property to Posts (Many-to-Many)
    public ICollection<Post> Posts { get; private set; } = [];

    private Tag() { } // EF Core

    public Tag(Guid id, string name, string slug)
    {
        Id = id;
        Name = name;
        Slug = slug;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}