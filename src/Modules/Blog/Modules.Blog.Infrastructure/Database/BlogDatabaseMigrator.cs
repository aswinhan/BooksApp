using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Infrastructure.Database;

public class BlogDatabaseMigrator : IModuleDatabaseMigrator
{
    public async Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}