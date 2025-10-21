using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Entities; // Needed for Author

namespace Modules.Catalog.Infrastructure.Database.Mapping;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey(a => a.Id); // Primary Key

        builder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(200); // Set max length

        builder.Property(a => a.Biography)
               .HasMaxLength(2000); // Optional, longer text

        // Configure the one-to-many relationship with Book
        // The relationship is defined on the Book side with HasForeignKey
        // No explicit configuration needed here if navigation property exists
        // builder.HasMany(a => a.Books)... is configured in BookConfiguration
    }
}