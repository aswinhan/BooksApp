using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Infrastructure.Database;
using Modules.Common.Infrastructure.Policies;
using Modules.Discounts.Infrastructure.Database;
using Modules.Discounts.Infrastructure.Policies;

// ReSharper disable once CheckNamespace
namespace Modules.Discounts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDiscountsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<DiscountsDbContext>((sp, opt) => {
            var interceptor = sp.GetRequiredService<AuditableInterceptor>();
            opt.UseNpgsql(connectionString, npgOpt => npgOpt.MigrationsHistoryTable(DbConsts.MigrationTableName, DbConsts.Schema))
               .AddInterceptors(interceptor)
               .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IModuleDatabaseMigrator, DiscountsDatabaseMigrator>();
        services.AddSingleton<IPolicyFactory, DiscountsPolicyFactory>();

        // Register repositories/services later if needed

        return services;
    }
}