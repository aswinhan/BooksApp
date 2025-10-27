using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.Infrastructure.Database;

public class DiscountsDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscountsDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}