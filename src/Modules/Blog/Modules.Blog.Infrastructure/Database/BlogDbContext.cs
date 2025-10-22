using Microsoft.EntityFrameworkCore;
using Modules.Blog.Domain.Entities; // Needed for DbSets

namespace Modules.Blog.Infrastructure.Database;

public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DbConsts.Schema); // Set schema
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly); // Apply mappings
    }
}