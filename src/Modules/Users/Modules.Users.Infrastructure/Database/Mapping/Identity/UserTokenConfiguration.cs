using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Users;

namespace Modules.Users.Infrastructure.Database.Mapping.Identity;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        // builder.ToTable("user_tokens"); // Naming convention handles this
        // Define composite primary key
        builder.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
    }
}