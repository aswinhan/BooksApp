using Microsoft.AspNetCore.Builder; // Required for IApplicationBuilder

namespace Modules.Common.API.Abstractions;

/// <summary>
/// Interface for classes that configure module-specific middleware.
/// </summary>
public interface IModuleMiddlewareConfigurator
{
    /// <summary>
    /// Configures module-specific middleware on the application pipeline.
    /// </summary>
    /// <param name="app">The IApplicationBuilder instance.</param>
    /// <returns>The configured IApplicationBuilder.</returns>
    IApplicationBuilder Configure(IApplicationBuilder app);
}