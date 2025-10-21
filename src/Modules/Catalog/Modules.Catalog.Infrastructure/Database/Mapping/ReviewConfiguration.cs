using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Entities; // Needed for Review
using Modules.Catalog.Domain.ValueObjects; // Needed for Rating

namespace Modules.Catalog.Infrastructure.Database.Mapping;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id); // Primary Key

        builder.Property(r => r.UserId)
               .IsRequired(); // Foreign key to User (string)

        builder.Property(r => r.Comment)
               .IsRequired()
               .HasMaxLength(1000);

        // Configure the owned Value Object 'Rating'
        builder.OwnsOne(r => r.Rating, ratingBuilder =>
        {
            // Map the 'Value' property of the Rating object
            // to a specific column name in the Reviews table.
            ratingBuilder.Property(rating => rating.Value)
                         .HasColumnName("rating_value") // Use snake_case
                         .IsRequired();
        });

        // Foreign key relationship back to Book is configured in BookConfiguration
        // builder.HasOne(r => r.Book)...
    }
}