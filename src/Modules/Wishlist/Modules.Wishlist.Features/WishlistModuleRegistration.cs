using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Extensions;
using Modules.Wishlist.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Modules.Wishlist.Features;

public static class WishlistModuleRegistration
{
    // public static string ActivityModuleName => "Wishlist";

    public static IServiceCollection AddWishlistModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddWishlistModuleApi()
            .AddWishlistInfrastructure(configuration);
    }

    private static IServiceCollection AddWishlistModuleApi(this IServiceCollection services)
    {
        // Register Internal/Public API if needed
        // ...

        services.RegisterApiEndpointsFromAssemblyContaining(typeof(WishlistModuleRegistration));
        services.RegisterHandlersFromAssemblyContaining(typeof(WishlistModuleRegistration));
        services.AddValidatorsFromAssembly(typeof(WishlistModuleRegistration).Assembly, includeInternalTypes: true);
        // services.AddSingleton<IModuleMiddlewareConfigurator, WishlistMiddlewareConfigurator>();

        return services;
    }
}

// Add Middleware Configurator if needed
// public class WishlistMiddlewareConfigurator : IModuleMiddlewareConfigurator { ... }