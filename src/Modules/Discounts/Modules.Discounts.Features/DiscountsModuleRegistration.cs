using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Extensions;
using Modules.Discounts.Features.InternalApi;
using Modules.Discounts.Infrastructure;
using Modules.Discounts.PublicApi; // Needed for AddDiscountsInfrastructure
// using Modules.Discounts.Features.InternalApi; // Add if creating internal API
// using Modules.Discounts.Features.InternalApi.Decorators; // Add if creating decorator
// using Modules.Discounts.PublicApi; // Add if creating public API
// using Modules.Discounts.Features.Tracing; // Add if creating tracing

// ReSharper disable once CheckNamespace
namespace Modules.Discounts.Features;

public static class DiscountsModuleRegistration
{
    // public static string ActivityModuleName => DiscountsActivitySource.ActivitySourceName; // Add if tracing

    public static IServiceCollection AddDiscountsModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddDiscountsModuleApi()
            .AddDiscountsInfrastructure(configuration);
    }

    private static IServiceCollection AddDiscountsModuleApi(this IServiceCollection services)
    {
        // --- Register Internal/Public API ---
        services.AddScoped<DiscountsModuleApi>(); // Register concrete
        services.AddScoped<IDiscountsModuleApi>(provider => { // Register interface
            var api = provider.GetRequiredService<DiscountsModuleApi>();
            // return new TracedDiscountsModuleApi(api); // Apply decorator if created
            return api;
        });
        // --- End Register API ---

        services.RegisterApiEndpointsFromAssemblyContaining(typeof(DiscountsModuleRegistration));
        services.RegisterHandlersFromAssemblyContaining(typeof(DiscountsModuleRegistration));
        services.AddValidatorsFromAssembly(typeof(DiscountsModuleRegistration).Assembly, includeInternalTypes: true);
        // services.AddSingleton<IModuleMiddlewareConfigurator, DiscountsMiddlewareConfigurator>(); // Add if middleware needed

        return services;
    }
}

// Add Middleware Configurator if needed
// public class DiscountsMiddlewareConfigurator : IModuleMiddlewareConfigurator { ... }