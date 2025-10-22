using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Blog.Domain.Entities; // Needed for Comment

namespace Modules.Blog.Infrastructure.Database.Mapping;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AuthorId).IsRequired();

        builder.Property(c => c.AuthorName)
               .IsRequired()
               .HasMaxLength(256); // Match User DisplayName max length?

        builder.Property(c => c.Content)
               .IsRequired()
               .HasMaxLength(2000); // Allow longer comments

        // Foreign key relationship back to Post is configured in PostConfiguration
    }
}