using Microsoft.EntityFrameworkCore;
using Modules.Discounts.Domain.Entities; // Needed for DbSet

namespace Modules.Discounts.Infrastructure.Database;

public class DiscountsDbContext(DbContextOptions<DiscountsDbContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(DbConsts.Schema); // Set schema
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiscountsDbContext).Assembly); // Apply mappings
    }
}