using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain.Entities; // Needed for DbSets

namespace Modules.Catalog.Infrastructure.Database;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    // DbSets represent the tables for our entities
    public virtual DbSet<Book> Books { get; set; } = null!;
    public virtual DbSet<Author> Authors { get; set; } = null!;
    public virtual DbSet<Review> Reviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set the default database schema for tables in this DbContext
        modelBuilder.HasDefaultSchema(DbConsts.Schema);

        // Apply all IEntityTypeConfiguration classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}