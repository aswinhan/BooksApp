using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database; // Use IModuleDatabaseMigrator
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Infrastructure.Database;

public class CatalogDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        // Resolve the specific DbContext for this module
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        // Apply pending migrations
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}