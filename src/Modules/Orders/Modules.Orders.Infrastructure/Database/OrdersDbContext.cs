using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain.Entities; // Needed for DbSets

namespace Modules.Orders.Infrastructure.Database;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DbConsts.Schema); // Set schema
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly); // Apply mappings
    }
}