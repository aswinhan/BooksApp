using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Blog.Domain.Entities; // Needed for Post

namespace Modules.Blog.Infrastructure.Database.Mapping;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(p => p.Content)
               .IsRequired(); // No max length by default (maps to TEXT in PostgreSQL)

        builder.Property(p => p.AuthorId).IsRequired();

        builder.Property(p => p.AuthorName)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(p => p.Slug)
               .IsRequired()
               .HasMaxLength(250);
        builder.HasIndex(p => p.Slug).IsUnique(); // Ensure slugs are unique

        builder.Property(p => p.IsPublished).IsRequired();
        builder.Property(p => p.PublishedAtUtc); // Nullable DateTime

        // One-to-Many with BlogCategory
        builder.HasOne(p => p.BlogCategory)
               .WithMany(c => c.Posts) // BlogCategory has many Posts
               .HasForeignKey(p => p.BlogCategoryId)
               .IsRequired();

        // Many-to-Many with Tag
        builder.HasMany(p => p.Tags)
               .WithMany(t => t.Posts)
               // EF Core 7+ will auto-generate the join table "post_tag"
               .UsingEntity(j => j.ToTable("post_tags"));

        // Configure the one-to-many relationship with Comment
        // Map the private '_comments' field
        builder.HasMany(p => p.Comments) // Post has many Comments
               .WithOne(c => c.Post) // Comment has one Post
               .HasForeignKey(c => c.PostId) // Foreign key in Comment
               .IsRequired();
        builder.Navigation(p => p.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}