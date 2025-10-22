using FluentValidation;
using Microsoft.AspNetCore.Builder; // Required for IApplicationBuilder
using Microsoft.Extensions.Configuration; // Required for IConfiguration
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Features.Books.GetBookById;
using Modules.Catalog.Features.Decorators;
using Modules.Catalog.Features.InternalApi;
using Modules.Catalog.Features.InternalApi.Decorators;
using Modules.Catalog.Infrastructure;
using Modules.Catalog.PublicApi;
using Modules.Common.API.Abstractions; // Required for IModuleMiddlewareConfigurator
using Modules.Common.API.Extensions;
using Modules.Common.Application.Caching;
using Modules.Common.Application.Extensions; // For RegisterHandlersFromAssemblyContaining

// Put extensions into the global Microsoft namespace for easy discovery
// ReSharper disable once CheckNamespace
namespace Modules.Catalog.Features;

public static class CatalogModuleRegistration
{
    // Define ActivityModuleName for OpenTelemetry if needed
    // public static string ActivityModuleName => "Catalog"; // Example

    /// <summary>
    /// Registers all services (API endpoints, handlers, validators, infrastructure)
    /// for the Catalog module.
    /// </summary>
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Chain registrations: Add API/Features services, then add Infrastructure services
        return services
            .AddCatalogModuleApi() // Registers endpoints, handlers, validators
            .AddCatalogInfrastructure(configuration); // Registers DbContext, services
    }

    // Registers services defined within the Features/API layer of the Catalog module
    private static IServiceCollection AddCatalogModuleApi(this IServiceCollection services)
    {
        // Register the actual implementation
        services.AddScoped<CatalogModuleApi>();

        // Register the Public API interface (ICatalogModuleApi)
        // Resolve the actual implementation and wrap it with the decorator
        services.AddScoped<ICatalogModuleApi>(provider =>
        {
            var actualImplementation = provider.GetRequiredService<CatalogModuleApi>();
            // Apply the Tracing decorator (we'll create this next)
            return new TracedCatalogModuleApi(actualImplementation);
        });

        // Automatically find and register all IApiEndpoint implementations
        services.RegisterApiEndpointsFromAssemblyContaining(typeof(CatalogModuleRegistration));

        // Automatically find and register all IHandler and IEventHandler implementations
        //services.RegisterHandlersFromAssemblyContaining(typeof(CatalogModuleRegistration));


        // --- Handler Registration with Caching ---

        // 1. Register the REAL handler implementation
        services.AddScoped<GetBookByIdHandler>();

        // 2. Register the INTERFACE (IGetBookByIdHandler) using a factory
        //    This factory resolves the real handler and the caching service,
        //    then creates the decorator instance, effectively wrapping the real handler.
        services.AddScoped<IGetBookByIdHandler>(provider =>
            new CachingGetBookByIdHandlerDecorator(
                provider.GetRequiredService<GetBookByIdHandler>(), // Get the real handler
                provider.GetRequiredService<ICachingService>()   // Get the caching service
            ));

        // Register OTHER handlers directly (if they don't need caching yet)
        // Find non-GetBookById handlers and register them
        var handlerTypesToRegisterDirectly = AssemblyReference.Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.IsAssignableTo(typeof(Modules.Common.Domain.Handlers.IHandler))
                        && !t.IsAssignableTo(typeof(Modules.Common.Domain.Events.IEventHandler)) // Exclude event handlers
                        && t != typeof(GetBookByIdHandler) // Exclude the one we decorated
                        && t != typeof(CachingGetBookByIdHandlerDecorator) // Exclude the decorator itself
                        )
            .ToList();

        foreach (var implementationType in handlerTypesToRegisterDirectly)
        {
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i != typeof(Modules.Common.Domain.Handlers.IHandler)
                                   && i.IsAssignableTo(typeof(Modules.Common.Domain.Handlers.IHandler)));
            if (interfaceType is not null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }

        // --- End of Handler Registration ---


        // Automatically find and register all FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(CatalogModuleRegistration).Assembly, includeInternalTypes: true);

        // Register middleware configurator (uncomment if using Tracing middleware)
        services.AddSingleton<IModuleMiddlewareConfigurator, CatalogMiddlewareConfigurator>();




        return services;
    }
}

// --- Optional: Middleware Configurator (Example for Tracing) ---
// Uncomment and implement if Catalog needs specific middleware like tracing

// public class CatalogMiddlewareConfigurator : IModuleMiddlewareConfigurator
// {
//     public IApplicationBuilder Configure(IApplicationBuilder app)
//     {
//         // Example: Add tracing middleware specific to catalog routes
//         // app.UseMiddleware<CatalogTracingMiddleware>();
//         return app;
//     }
// }