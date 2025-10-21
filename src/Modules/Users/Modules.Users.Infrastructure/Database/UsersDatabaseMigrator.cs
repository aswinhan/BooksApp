using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database; // Use IModuleDatabaseMigrator
using System.Threading;
using System.Threading.Tasks;


namespace Modules.Users.Infrastructure.Database;

public class UsersDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        // Resolve the specific DbContext for this module
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Apply pending migrations
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}