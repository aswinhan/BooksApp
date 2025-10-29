using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Wishlist.Infrastructure.Database;

public class WishlistDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WishlistDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}