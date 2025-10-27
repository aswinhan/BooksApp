using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application; // Required for EventPublisher
using Modules.Common.Application.Extensions;
using Modules.Common.Domain.Events;
using Modules.Orders.Features.InternalApi;
using Modules.Orders.Features.Webhooks;
using Modules.Orders.Infrastructure;
using Modules.Orders.PublicApi;

// Put extensions into the global Microsoft namespace
// ReSharper disable once CheckNamespace
namespace Modules.Orders.Features;

public static class OrdersModuleRegistration
{
    // Define ActivityModuleName for OpenTelemetry if needed
    // public static string ActivityModuleName => "Orders";

    /// <summary>
    /// Registers all services for the Orders module.
    /// </summary>
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddOrdersModuleApi()
            .AddOrdersInfrastructure(configuration);
    }

    private static IServiceCollection AddOrdersModuleApi(this IServiceCollection services)
    {
        // Register the shared EventPublisher implementation (used by CheckoutHandler)
        // It's scoped because it depends on IServiceProvider to resolve handlers
        services.AddScoped<IEventPublisher, EventPublisher>();

        // Automatically find and register API endpoints
        services.RegisterApiEndpointsFromAssemblyContaining(typeof(OrdersModuleRegistration));

        // Automatically find and register handlers
        services.RegisterHandlersFromAssemblyContaining(typeof(OrdersModuleRegistration));

        // --- Register Internal API ---
        services.AddScoped<OrdersModuleApi>(); // Register concrete implementation
        services.AddScoped<IOrdersModuleApi>(provider => { // Register interface
            var api = provider.GetRequiredService<OrdersModuleApi>();
            // return new TracedOrdersModuleApi(api); // Apply tracing decorator if created
            return api; // Return concrete for now
        });
        // --- End Register Internal API ---

        // Automatically find and register validators
        services.AddValidatorsFromAssembly(typeof(OrdersModuleRegistration).Assembly, includeInternalTypes: true);

        // Register middleware configurator if needed
        // services.AddSingleton<IModuleMiddlewareConfigurator, OrdersMiddlewareConfigurator>();

        // --- Register Webhook Services ---
        services.AddScoped<StripeEventHandlerFactory>(); // Factory
        services.AddScoped<PaymentIntentSucceededHandler>(); // Specific handlers
        services.AddScoped<PaymentIntentFailedHandler>();
        // Register other handlers here
        // --- End Webhook Services ---

        return services;
    }
}

// --- Optional: Middleware Configurator ---
// Uncomment and implement if Orders needs specific middleware

// public class OrdersMiddlewareConfigurator : IModuleMiddlewareConfigurator
// {
//     public IApplicationBuilder Configure(IApplicationBuilder app)
//     {
//         // app.UseMiddleware<SomeOrdersMiddleware>();
//         return app;
//     }
// }