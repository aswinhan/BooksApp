using Microsoft.EntityFrameworkCore;
using Modules.Wishlist.Domain.Entities; // Needed for DbSet

namespace Modules.Wishlist.Infrastructure.Database;

public class WishlistDbContext(DbContextOptions<WishlistDbContext> options) : DbContext(options)
{
    public DbSet<WishlistItem> WishlistItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(DbConsts.Schema); // Set schema
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WishlistDbContext).Assembly); // Apply mappings
    }
}