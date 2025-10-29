using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Blog.Domain.Entities;

namespace Modules.Blog.Infrastructure.Database.Mapping;

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(150);
        builder.HasIndex(c => c.Slug).IsUnique();
    }
}