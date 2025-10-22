using Modules.Common.Domain; // For IAuditableEntity
using System;

namespace Modules.Blog.Domain.Entities;

public class Comment : IAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; } // Foreign key to Post
    public string AuthorId { get; private set; } = null!; // Foreign key to User (string)
    public string AuthorName { get; private set; } = null!; // Denormalized author name
    public string Content { get; private set; } = null!;

    // Navigation property back to Post (optional)
    public Post Post { get; private set; } = null!;

    // Private constructor for EF Core
    private Comment() { }

    // Internal constructor - only Post aggregate should create comments
    internal Comment(Guid id, Guid postId, string authorId, string authorName, string content)
    {
        Id = id;
        PostId = postId;
        AuthorId = authorId;
        AuthorName = authorName; // Store author name at time of comment
        Content = content;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // Method to update comment content (optional)
    internal void UpdateContent(string newContent)
    {
        Content = newContent;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}