using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql; // For NpgsqlConnection
using Respawn; // For database cleanup
using StackExchange.Redis; // For IConnectionMultiplexer
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace BooksApp.Tests.Integration;

// This class sets up our entire application stack for testing
public class BooksAppApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Containers for our external dependencies
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithDatabase("booksapp_test_db")
    .WithUsername("testuser")
    .WithPassword("testpass")
    .WithWaitStrategy(Wait.ForUnixContainer()) // Removed .UntilPortIsAvailable(5432)
    .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .WithWaitStrategy(Wait.ForUnixContainer()) // Removed .UntilPortIsAvailable(6379)
        .Build();

    // This Respawner instance helps clean the database
    private Respawner _respawner = default!;
    private NpgsqlConnection _dbConnection = default!;

    // This method configures the services *inside* our app
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // 1. Remove all existing DbContext registrations
            // This stops the app from trying to connect to the "real" database
            services.RemoveAll<DbContextOptions<Modules.Users.Infrastructure.Database.UsersDbContext>>();
            services.RemoveAll<DbContextOptions<Modules.Catalog.Infrastructure.Database.CatalogDbContext>>();
            services.RemoveAll<DbContextOptions<Modules.Orders.Infrastructure.Database.OrdersDbContext>>();
            services.RemoveAll<DbContextOptions<Modules.Blog.Infrastructure.Database.BlogDbContext>>();
            services.RemoveAll<DbContextOptions<Modules.Inventory.Infrastructure.Database.InventoryDbContext>>();
            services.RemoveAll<DbContextOptions<Modules.Discounts.Infrastructure.Database.DiscountsDbContext>>();
            services.RemoveAll<DbContextOptions<Modules.Wishlist.Infrastructure.Database.WishlistDbContext>>();


            // 2. Add DbContexts pointing to the *test container*
            string postgresConnectionString = _postgresContainer.GetConnectionString();
            services.AddDbContext<Modules.Users.Infrastructure.Database.UsersDbContext>(opt => opt.UseNpgsql(postgresConnectionString).UseSnakeCaseNamingConvention());
            services.AddDbContext<Modules.Catalog.Infrastructure.Database.CatalogDbContext>(opt => opt.UseNpgsql(postgresConnectionString).UseSnakeCaseNamingConvention());
            services.AddDbContext<Modules.Orders.Infrastructure.Database.OrdersDbContext>(opt => opt.UseNpgsql(postgresConnectionString).UseSnakeCaseNamingConvention());
            services.AddDbContext<Modules.Blog.Infrastructure.Database.BlogDbContext>(opt => opt.UseNpgsql(postgresConnectionString).UseSnakeCaseNamingConvention());
            services.AddDbContext<Modules.Inventory.Infrastructure.Database.InventoryDbContext>(opt => opt.UseNpgsql(postgresConnectionString).UseSnakeCaseNamingConvention());
            services.AddDbContext<Modules.Discounts.Infrastructure.Database.DiscountsDbContext>(opt => opt.UseNpgsql(postgresConnectionString).UseSnakeCaseNamingConvention());
            services.AddDbContext<Modules.Wishlist.Infrastructure.Database.WishlistDbContext>(opt => opt.UseNpgsql(postgresConnectionString).UseSnakeCaseNamingConvention());


            // 3. Remove the "real" Redis connection
            services.RemoveAll<IConnectionMultiplexer>();

            // 4. Add the *test container's* Redis connection
            string redisConnectionString = _redisContainer.GetConnectionString();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        });
    }

    // This method runs ONCE before all tests in the class
    public async Task InitializeAsync()
    {
        // Start the containers
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        // Run migrations to create all schemas and tables in the test database
        using var scope = Services.CreateScope();
        var migrators = scope.ServiceProvider.GetServices<Modules.Common.Infrastructure.Database.IModuleDatabaseMigrator>();
        foreach (var migrator in migrators)
        {
            await migrator.MigrateAsync(scope);
        }

        // Prepare Respawn to clean the database
        _dbConnection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["users", "catalog", "orders", "blog", "inventory", "discounts", "wishlist"]
        });
    }

    // This method runs ONCE after all tests in the class
    public new async Task DisposeAsync()
    {
        await _dbConnection.CloseAsync();
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }

    // This is a helper method we'll use in our tests
    public async Task ResetDatabaseAsync()
    {
        // Clear all data from all tables in our schemas
        await _respawner.ResetAsync(_dbConnection);
    }
}