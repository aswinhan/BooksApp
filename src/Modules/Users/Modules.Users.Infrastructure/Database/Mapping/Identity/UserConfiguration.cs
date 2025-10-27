using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Users;

namespace Modules.Users.Infrastructure.Database.Mapping.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // builder.ToTable("users"); // Naming convention handles this

        // Configure the relationships using the navigation properties

        builder.HasMany(e => e.Claims)       // User has many Claims
            .WithOne(e => e.User)           // Each Claim belongs to one User
            .HasForeignKey(uc => uc.UserId) // Foreign key in UserClaim
            .IsRequired();

        builder.HasMany(e => e.Logins)       // User has many Logins
            .WithOne(e => e.User)           // Each Login belongs to one User
            .HasForeignKey(ul => ul.UserId) // Foreign key in UserLogin
            .IsRequired();

        builder.HasMany(e => e.Tokens)       // User has many Tokens
            .WithOne(e => e.User)           // Each Token belongs to one User
            .HasForeignKey(ut => ut.UserId) // Foreign key in UserToken
            .IsRequired();

        // Configure the many-to-many join entity UserRole
        builder.HasMany(e => e.UserRoles)    // User has many UserRoles
            .WithOne(e => e.User)           // Each UserRole belongs to one User
            .HasForeignKey(ur => ur.UserId) // Foreign key in UserRole
            .IsRequired();

        // Configure DisplayName property if needed (e.g., max length)
        builder.Property(u => u.DisplayName).HasMaxLength(256);

        builder.Property(u => u.Street).HasMaxLength(200);
        builder.Property(u => u.City).HasMaxLength(100);
        builder.Property(u => u.State).HasMaxLength(100);
        builder.Property(u => u.ZipCode).HasMaxLength(20);
        builder.Property(u => u.Country).HasMaxLength(100);
    }
}