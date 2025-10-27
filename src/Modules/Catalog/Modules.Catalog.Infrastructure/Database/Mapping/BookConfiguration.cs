using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Entities; // Needed for Book

namespace Modules.Catalog.Infrastructure.Database.Mapping;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id); // Primary Key

        builder.Property(b => b.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(b => b.Description)
               .HasMaxLength(2000);

        builder.Property(b => b.Isbn)
               .IsRequired()
               .HasMaxLength(13); // Standard ISBN length

        builder.Property(b => b.Price)
               .HasColumnType("decimal(18,2)") // Specify precision for currency
               .IsRequired();

        builder.Property(b => b.CoverImageUrl)
       .HasMaxLength(500); // Example max length for URL

        // Configure the many-to-one relationship with Author
        builder.HasOne(b => b.Author) // Book has one Author
               .WithMany(a => a.Books) // Author has many Books
               .HasForeignKey(b => b.AuthorId) // Foreign key property in Book
               .IsRequired();

        // Configure the many-to-one relationship with Category
        builder.HasOne(b => b.Category) // Book has one Category
               .WithMany(c => c.Books) // Category has many Books
               .HasForeignKey(b => b.CategoryId) // Foreign key in Book
               .IsRequired(false);

        // Configure the one-to-many relationship with Review
        // Map the private '_reviews' field using UsePropertyAccessMode
        builder.HasMany(b => b.Reviews) // Book has many Reviews
               .WithOne(r => r.Book) // Review has one Book
               .HasForeignKey(r => r.BookId) // Foreign key property in Review
               .IsRequired();
        builder.Navigation(b => b.Reviews).UsePropertyAccessMode(PropertyAccessMode.Field); // Map the private field
    }
}