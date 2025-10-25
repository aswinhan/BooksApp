using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // Add this
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Extensions;
using Modules.Inventory.Features.InternalApi; // Use internal API
using Modules.Inventory.Features.InternalApi.Decorators; // Use decorator
using Modules.Inventory.Features.Tracing; // Use tracing
using Modules.Inventory.Infrastructure; // Needed for AddInventoryInfrastructure
using Modules.Inventory.PublicApi; // Use public API

// ReSharper disable once CheckNamespace
namespace Modules.Inventory.Features;

public static class InventoryModuleRegistration
{
    public static string ActivityModuleName => InventoryActivitySource.ActivitySourceName;

    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddInventoryModuleApi()
            .AddInventoryInfrastructure(configuration);
    }

    private static IServiceCollection AddInventoryModuleApi(this IServiceCollection services)
    {
        services.AddScoped<InventoryModuleApi>(); // Register concrete implementation
        services.AddScoped<IInventoryModuleApi>(provider => { // Register interface with decorator
            var api = provider.GetRequiredService<InventoryModuleApi>();
            return new TracedInventoryModuleApi(api); // Apply tracing
        });

        services.RegisterApiEndpointsFromAssemblyContaining(typeof(InventoryModuleRegistration));
        services.RegisterHandlersFromAssemblyContaining(typeof(InventoryModuleRegistration));
        services.AddValidatorsFromAssembly(typeof(InventoryModuleRegistration).Assembly, includeInternalTypes: true);
        // services.AddSingleton<IModuleMiddlewareConfigurator, InventoryMiddlewareConfigurator>(); // Add if tracing middleware needed

        return services;
    }
}

// Add Middleware Configurator if needed
// public class InventoryMiddlewareConfigurator : IModuleMiddlewareConfigurator { ... }