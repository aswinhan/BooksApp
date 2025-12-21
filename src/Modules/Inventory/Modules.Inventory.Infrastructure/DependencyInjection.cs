using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // Add this
using Modules.Common.Infrastructure.Database;
using Modules.Common.Infrastructure.Policies;
using Modules.Inventory.Infrastructure.Database;
using Modules.Inventory.Infrastructure.Policies;

// ReSharper disable once CheckNamespace
namespace Modules.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Matches the .AddDatabase("booksappdb") in AppHost
        var connectionString = configuration.GetConnectionString("booksappdb"); // Use the main connection string

        // Safety check (Optional but good for debugging)
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find connection string 'booksappdb'");
        }

        services.AddDbContext<InventoryDbContext>((sp, opt) => {
            var interceptor = sp.GetRequiredService<AuditableInterceptor>();
            opt.UseNpgsql(connectionString, npgOpt => npgOpt.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema))
               .AddInterceptors(interceptor)
               .UseSnakeCaseNamingConvention();
        });
        services.AddScoped<IModuleDatabaseMigrator, InventoryDatabaseMigrator>();
        services.AddSingleton<IPolicyFactory, InventoryPolicyFactory>();
        // Register repositories later if needed
        return services;
    }
}