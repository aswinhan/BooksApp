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
        var connectionString = configuration.GetConnectionString("Postgres");
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