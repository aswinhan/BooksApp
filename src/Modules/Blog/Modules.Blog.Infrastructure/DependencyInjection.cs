using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Blog.Infrastructure.Database;
using Modules.Blog.Infrastructure.Policies;
using Modules.Common.Infrastructure.Database;
using Modules.Common.Infrastructure.Policies;

// Put extensions into the global Microsoft namespace
// ReSharper disable once CheckNamespace
namespace Modules.Blog.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers services for the Blog module's infrastructure layer.
    /// </summary>
    public static IServiceCollection AddBlogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        // Add the DbContext specific to this module
        services.AddDbContext<BlogDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<AuditableInterceptor>();
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema);
                })
                .AddInterceptors(interceptor)
                .UseSnakeCaseNamingConvention();
        });

        // Register the migrator for this module
        services.AddScoped<IModuleDatabaseMigrator, BlogDatabaseMigrator>();

        // Register this module's policy factory
        services.AddSingleton<IPolicyFactory, BlogPolicyFactory>();

        // Register Repositories or other infrastructure services here later if needed

        return services;
    }
}