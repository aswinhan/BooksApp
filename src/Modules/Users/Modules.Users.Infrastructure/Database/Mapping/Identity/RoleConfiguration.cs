using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Users;

namespace Modules.Users.Infrastructure.Database.Mapping.Identity;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // builder.ToTable("roles"); // Naming convention handles this

        // Configure relationships using navigation properties

        builder.HasMany(e => e.UserRoles)      // Role has many UserRoles
            .WithOne(e => e.Role)             // Each UserRole belongs to one Role
            .HasForeignKey(ur => ur.RoleId)   // Foreign key in UserRole
            .IsRequired();

        builder.HasMany(e => e.RoleClaims)     // Role has many RoleClaims
            .WithOne(e => e.Role)             // Each RoleClaim belongs to one Role
            .HasForeignKey(rc => rc.RoleId)   // Foreign key in RoleClaim
            .IsRequired();
    }
}