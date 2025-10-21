using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Tokens;

namespace Modules.Users.Infrastructure.Database.Mapping;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Set the table name explicitly if needed, otherwise EF uses DbSet name
        // builder.ToTable("refresh_tokens"); // Naming convention handles this

        builder.HasKey(e => e.Token); // Primary Key is the Token itself

        builder.Property(e => e.JwtId).IsRequired();
        builder.Property(e => e.ExpiryDate).IsRequired();
        builder.Property(e => e.Invalidated).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc); // Nullable

        // Configure the relationship to the User entity
        builder.HasOne(e => e.User)
            .WithMany() // No navigation property back from User needed
            .HasForeignKey(e => e.UserId)
            .IsRequired(); // Foreign key is required
    }
}