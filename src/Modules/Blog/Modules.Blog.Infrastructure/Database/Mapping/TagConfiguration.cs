using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Blog.Domain.Entities;

namespace Modules.Blog.Infrastructure.Database.Mapping;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(60);
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}