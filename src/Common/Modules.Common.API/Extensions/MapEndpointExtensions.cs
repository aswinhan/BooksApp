using Microsoft.AspNetCore.Builder; // Required for WebApplication
using Microsoft.Extensions.DependencyInjection; // Required for IServiceCollection, GetRequiredService
using Microsoft.Extensions.DependencyInjection.Extensions; // Required for TryAddEnumerable
using Modules.Common.API.Abstractions; // Required for IApiEndpoint
using System; // Required for Type
using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for Where, Select, ToArray
using System.Reflection; // Required for Assembly

// Put extensions into Microsoft.Extensions.DependencyInjection for easy discovery
// ReSharper disable once CheckNamespace
namespace Modules.Common.API.Extensions;


public static class MapEndpointExtensions
{
    /// <summary>
    /// Scans an assembly for classes implementing IApiEndpoint and registers them.
    /// </summary>
    public static IServiceCollection RegisterApiEndpointsFromAssemblyContaining(this IServiceCollection services, Type marker)
    {
        var assembly = marker.Assembly;

        // Find all concrete classes implementing IApiEndpoint
        var endpointTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IApiEndpoint)) && t is { IsClass: true, IsAbstract: false, IsInterface: false });

        // Create service descriptors to register each endpoint class as an IApiEndpoint service
        var serviceDescriptors = endpointTypes
            .Select(type => ServiceDescriptor.Transient(typeof(IApiEndpoint), type)) // Register as Transient
            .ToArray();

        // Add the descriptors to the service collection if not already registered
        services.TryAddEnumerable(serviceDescriptors);
        return services;
    }

    /// <summary>
    /// Finds all registered IApiEndpoint services and calls their MapEndpoint method.
    /// </summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        // Resolve all registered IApiEndpoint instances
        var endpoints = app.Services.GetRequiredService<IEnumerable<IApiEndpoint>>();

        // Call MapEndpoint for each one
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}