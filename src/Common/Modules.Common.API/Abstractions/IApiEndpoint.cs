using Microsoft.AspNetCore.Builder; // Required for WebApplication

namespace Modules.Common.API.Abstractions;

/// <summary>
/// Interface for classes that define and map API endpoints for a module.
/// </summary>
public interface IApiEndpoint
{
    /// <summary>
    /// Maps the module's specific endpoints to the main web application.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    void MapEndpoint(WebApplication app);
}