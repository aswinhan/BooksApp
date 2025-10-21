using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic; // For IEnumerable
using System.Threading; // For CancellationToken
using System.Threading.Tasks; // For Task

namespace Modules.Common.Infrastructure.Database;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Finds all registered IModuleDatabaseMigrator services and runs their MigrateAsync method.
    /// </summary>
    public static async Task MigrateModuleDatabasesAsync(this IServiceScope scope,
        CancellationToken cancellationToken = default)
    {
        // Get all registered migrator services
        var migrators = scope.ServiceProvider.GetRequiredService<IEnumerable<IModuleDatabaseMigrator>>();

        // Run migrations for each module
        foreach (var migrator in migrators)
        {
            await migrator.MigrateAsync(scope, cancellationToken);
        }
    }
}