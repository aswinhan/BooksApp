using FluentValidation; // For AddValidatorsFromAssembly
using Microsoft.AspNetCore.Builder; // For IApplicationBuilder
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.API.Abstractions; // For IModuleMiddlewareConfigurator
using Modules.Common.API.Extensions;
using Modules.Common.Application.Extensions; // For RegisterHandlersFromAssemblyContaining
using Modules.Users.Features.Middlewares;
using Modules.Users.Infrastructure; // For CheckRevocatedTokensMiddleware

// Put extensions into the global Microsoft namespace for easy discovery
// ReSharper disable once CheckNamespace
namespace Modules.Users.Features; // Naming convention change

public static class UsersModuleRegistration
{
    /// <summary>
    /// Registers all services (API endpoints, handlers, validators, infrastructure)
    /// for the Users module.
    /// </summary>
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Chain registrations: Add API/Features services, then add Infrastructure services
        return services
            .AddUsersModuleApi() // Registers endpoints, handlers, validators
            .AddUsersInfrastructure(configuration); // Registers DbContext, Identity, services
    }

    // Registers services defined within the Features/API layer of the Users module
    private static IServiceCollection AddUsersModuleApi(this IServiceCollection services)
    {
        // Automatically find and register all IApiEndpoint implementations in this assembly
        services.RegisterApiEndpointsFromAssemblyContaining(typeof(UsersModuleRegistration));

        // Automatically find and register all IHandler and IEventHandler implementations
        services.RegisterHandlersFromAssemblyContaining(typeof(UsersModuleRegistration));

        // Automatically find and register all FluentValidation validators in this assembly
        services.AddValidatorsFromAssembly(typeof(UsersModuleRegistration).Assembly, includeInternalTypes: true); // Include internal handlers/validators

        // Register the middleware configurator for this module
        services.AddSingleton<IModuleMiddlewareConfigurator, UsersMiddlewareConfigurator>();


        return services;
    }
}

// Configures middleware specific to the Users module
public class UsersMiddlewareConfigurator : IModuleMiddlewareConfigurator
{
    public IApplicationBuilder Configure(IApplicationBuilder app)
    {
        // Add the token revocation check middleware to the pipeline
        return app.UseMiddleware<CheckRevocatedTokensMiddleware>();
    }
}