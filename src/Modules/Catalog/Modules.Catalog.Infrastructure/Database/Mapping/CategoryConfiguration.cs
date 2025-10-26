﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Entities;

namespace Modules.Catalog.Infrastructure.Database.Mapping;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.Property(c => c.Slug).IsRequired().HasMaxLength(150);
        builder.HasIndex(c => c.Slug).IsUnique(); // Ensure slugs are unique

        // If implementing hierarchy:
        // builder.HasOne(c => c.ParentCategory)
        //        .WithMany(c => c.Subcategories)
        //        .HasForeignKey(c => c.ParentCategoryId)
        //        .OnDelete(DeleteBehavior.Restrict); // Prevent deleting parent if children exist
    }
}