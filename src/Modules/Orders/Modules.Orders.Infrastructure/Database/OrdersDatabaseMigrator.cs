using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Infrastructure.Database;

public class OrdersDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}