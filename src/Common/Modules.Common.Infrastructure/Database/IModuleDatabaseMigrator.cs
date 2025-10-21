using Microsoft.Extensions.DependencyInjection; // For IServiceScope
using System.Threading; // For CancellationToken
using System.Threading.Tasks; // For Task

namespace Modules.Common.Infrastructure.Database;

/// <summary>
/// Interface for services that can apply migrations for a specific module's DbContext.
/// </summary>
public interface IModuleDatabaseMigrator
{
    /// <summary>
    /// Applies pending migrations for the module's database.
    /// </summary>
    /// <param name="scope">The service scope to resolve the DbContext.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MigrateAsync(IServiceScope scope, CancellationToken cancellationToken = default);
}