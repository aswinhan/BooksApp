using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Base class
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Tokens; // Need DbSet for RefreshToken
using Modules.Users.Domain.Users; // Need our custom User, Role etc.

namespace Modules.Users.Infrastructure.Database;

// Inherit from IdentityDbContext with our custom entities and string key
public class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityDbContext<User, Role, string,
    UserClaim, UserRole, UserLogin, RoleClaim, UserToken>(options)
{
    // Add DbSet for our non-Identity entity
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT: Call base.OnModelCreating for Identity tables
        base.OnModelCreating(modelBuilder);

        // Set the default database schema for this DbContext
        modelBuilder.HasDefaultSchema(DbConsts.Schema);

        // Apply all IEntityTypeConfiguration classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}