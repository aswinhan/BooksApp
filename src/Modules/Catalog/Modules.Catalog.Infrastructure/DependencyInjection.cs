using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Common.Infrastructure.Database; // For IModuleDatabaseMigrator, AuditableInterceptor
using Modules.Common.Infrastructure.Policies; // For IPolicyFactory
using Modules.Catalog.Infrastructure.Database; // For CatalogDbContext, DbConsts etc.
using Modules.Catalog.Infrastructure.Policies;
using Microsoft.Extensions.DependencyInjection;


// Put extensions into the global Microsoft namespace for easy discovery
// ReSharper disable once CheckNamespace
namespace Modules.Catalog.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers services specific to the Catalog module's infrastructure layer.
    /// </summary>
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres"); // Use the main connection string

        // Add the DbContext specific to this module
        services.AddDbContext<CatalogDbContext>((serviceProvider, options) =>
        {
            // Resolve the shared AuditableInterceptor
            var interceptor = serviceProvider.GetRequiredService<AuditableInterceptor>();

            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // Configure migrations history table within the module's schema
                    npgsqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
                })
                .AddInterceptors(interceptor) // Add the interceptor
                .UseSnakeCaseNamingConvention(); // Apply naming convention
        });

        // Register the migrator for this module's DbContext
        services.AddScoped<IModuleDatabaseMigrator, CatalogDatabaseMigrator>();

        // Register this module's policy factory
        services.AddSingleton<IPolicyFactory, CatalogPolicyFactory>();

        // Register Repositories or other infrastructure services here later if needed
        // services.AddScoped<IBookRepository, BookRepository>();

        return services;
    }
}