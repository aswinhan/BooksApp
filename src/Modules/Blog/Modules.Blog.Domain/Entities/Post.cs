using Modules.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Blog.Domain.Entities;

public class Post : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string AuthorId { get; private set; } = null!; // User ID (string)
    public string AuthorName { get; private set; } = null!; // Denormalized
    public string Slug { get; private set; } = null!; // URL-friendly identifier
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }

    // --- ADD Category (One-to-Many) ---
    public Guid BlogCategoryId { get; private set; }
    public BlogCategory BlogCategory { get; private set; } = null!;
    // --- End Category ---

    // --- ADD Tags (Many-to-Many) ---
    public ICollection<Tag> Tags { get; private set; } = [];
    // --- End Tags ---

    // Private field for comments, exposed read-only
    private readonly List<Comment> _comments = [];
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    // Private constructor for EF Core
    private Post() { }

    // Public constructor for creating a new Post
    public Post(Guid id, string title, string content, string authorId, string authorName, string slug, Guid blogCategoryId)
    {
        // Add validation
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be empty.", nameof(content));
        if (string.IsNullOrWhiteSpace(authorId)) throw new ArgumentException("AuthorId cannot be empty.", nameof(authorId));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty.", nameof(slug)); // Consider generating slug automatically

        Id = id;
        Title = title;
        Content = content;
        AuthorId = authorId;
        AuthorName = authorName; // Denormalized
        Slug = slug; // Consider ensuring uniqueness
        BlogCategoryId = blogCategoryId;
        IsPublished = false; // Draft by default
        PublishedAtUtc = null;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- Aggregate Root Business Logic ---

    public void Update(string title, string content, string slug, Guid blogCategoryId)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content cannot be empty.", nameof(content));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty.", nameof(slug));

        Title = title;
        Content = content;
        Slug = slug;
        BlogCategoryId = blogCategoryId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // --- ADD Methods to manage Tags ---
    public void SetTags(List<Tag> tags)
    {
        Tags.Clear();
        Tags = tags; // EF Core will manage the join table
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (IsPublished) return; // Idempotent

        IsPublished = true;
        PublishedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        if (!IsPublished) return; // Idempotent

        IsPublished = false;
        PublishedAtUtc = null; // Clear published date
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddComment(string authorId, string authorName, string commentContent)
    {
        // Optional: Add rules like preventing comments on unpublished posts?
        // if (!IsPublished) throw new InvalidOperationException("Cannot comment on unpublished post.");

        var comment = new Comment(Guid.NewGuid(), this.Id, authorId, authorName, commentContent);
        _comments.Add(comment);
        UpdatedAtUtc = DateTime.UtcNow; // Mark post as updated
    }

    // Optional: Method to edit a comment (find by ID, call comment.UpdateContent)
    public void EditComment(Guid commentId, string userId, string newContent)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId) ?? throw new InvalidOperationException($"Comment {commentId} not found.");
        // Rule: Only the author can edit their comment?
        if (comment.AuthorId != userId) throw new InvalidOperationException("User cannot edit this comment.");

        comment.UpdateContent(newContent);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // Optional: Method to delete a comment (find by ID, remove from _comments)
    public void DeleteComment(Guid commentId, string userId)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is null) return; // Idempotent if not found

        // Rule: Only author or maybe post author/admin can delete?
        if (comment.AuthorId != userId && this.AuthorId != userId) // Example: Author or Post owner
        {
            throw new InvalidOperationException("User cannot delete this comment.");
        }

        _comments.Remove(comment);
        UpdatedAtUtc = DateTime.UtcNow;
    }


    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}