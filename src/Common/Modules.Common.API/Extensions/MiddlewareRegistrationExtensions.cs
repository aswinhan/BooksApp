using Microsoft.AspNetCore.Builder; // Required for IApplicationBuilder
using Microsoft.Extensions.DependencyInjection; // Required for GetServices
using Modules.Common.API.Abstractions; // Required for IModuleMiddlewareConfigurator
using System.Collections.Generic; // Required for IEnumerable

namespace Modules.Common.API.Extensions;

public static class MiddlewareRegistrationExtensions
{
    /// <summary>
    /// Finds all registered IModuleMiddlewareConfigurator services and runs their Configure method.
    /// </summary>
    public static IApplicationBuilder UseModuleMiddlewares(this IApplicationBuilder app)
    {
        // Resolve all registered middleware configurators
        var configurators = app.ApplicationServices.GetServices<IModuleMiddlewareConfigurator>();

        // Apply each configurator's middleware
        foreach (var configurator in configurators)
        {
            configurator.Configure(app); // Note: This assumes Configure returns void or IApplicationBuilder
        }

        return app;
    }
}