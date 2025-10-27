using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Application.Payments;
using Modules.Common.Infrastructure.Database;
using Modules.Common.Infrastructure.Policies;
using Modules.Orders.Domain.Abstractions; // For ICartService
using Modules.Orders.Infrastructure.Database;
using Modules.Orders.Infrastructure.Policies;
using Modules.Orders.Infrastructure.Services; // For RedisCartService

// Put extensions into the global Microsoft namespace
// ReSharper disable once CheckNamespace
namespace Modules.Orders.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers services for the Orders module's infrastructure layer.
    /// </summary>
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        // Add the DbContext specific to this module
        services.AddDbContext<OrdersDbContext>((serviceProvider, options) =>
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
        services.AddScoped<IModuleDatabaseMigrator, OrdersDatabaseMigrator>();

        // Register this module's policy factory
        services.AddSingleton<IPolicyFactory, OrdersPolicyFactory>();

        // Register the Redis implementation for ICartService
        // Assumes IConnectionMultiplexer is registered in Host/Common.Infrastructure
        services.AddScoped<ICartService, RedisCartService>();

        services.AddScoped<IPaymentService, StripePaymentService>();

        // Register Repositories or other infrastructure services here later
        // services.AddScoped<IOrderRepository, OrderRepository>();


        return services;
    }
}