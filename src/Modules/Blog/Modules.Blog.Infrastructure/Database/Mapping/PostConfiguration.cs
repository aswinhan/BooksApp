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

        // Configure the one-to-many relationship with Comment
        // Map the private '_comments' field
        builder.HasMany(p => p.Comments) // Post has many Comments
               .WithOne(c => c.Post) // Comment has one Post
               .HasForeignKey(c => c.PostId) // Foreign key in Comment
               .IsRequired();
        builder.Navigation(p => p.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}