using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Users;

namespace Modules.Users.Infrastructure.Database.Mapping.Identity;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // builder.ToTable("user_roles"); // Naming convention handles this

        // Define composite primary key for the join table
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });
    }
}