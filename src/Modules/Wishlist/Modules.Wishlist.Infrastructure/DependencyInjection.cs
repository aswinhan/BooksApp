using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database;
using Modules.Common.Infrastructure.Policies;
using Modules.Wishlist.Infrastructure.Database;
using Modules.Wishlist.Infrastructure.Policies;

// ReSharper disable once CheckNamespace
namespace Modules.Wishlist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWishlistInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<WishlistDbContext>((sp, opt) => {
            var interceptor = sp.GetRequiredService<AuditableInterceptor>();
            opt.UseNpgsql(connectionString, npgOpt => npgOpt.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema))
               .AddInterceptors(interceptor)
               .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IModuleDatabaseMigrator, WishlistDatabaseMigrator>();
        services.AddSingleton<IPolicyFactory, WishlistPolicyFactory>();

        // Register repositories/services later if needed

        return services;
    }
}