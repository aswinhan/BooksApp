using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Blog.Infrastructure;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Extensions;

// Put extensions into the global Microsoft namespace
// ReSharper disable once CheckNamespace
namespace Modules.Blog.Features;

public static class BlogModuleRegistration
{
    // Define ActivityModuleName for OpenTelemetry if needed
    // public static string ActivityModuleName => "Blog";

    /// <summary>
    /// Registers all services for the Blog module.
    /// </summary>
    public static IServiceCollection AddBlogModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddBlogModuleApi()
            .AddBlogInfrastructure(configuration);
    }

    private static IServiceCollection AddBlogModuleApi(this IServiceCollection services)
    {
        // Register API endpoints
        services.RegisterApiEndpointsFromAssemblyContaining(typeof(BlogModuleRegistration));

        // Register handlers
        services.RegisterHandlersFromAssemblyContaining(typeof(BlogModuleRegistration));

        // Register validators
        services.AddValidatorsFromAssembly(typeof(BlogModuleRegistration).Assembly, includeInternalTypes: true);

        // Register middleware configurator if needed
        // services.AddSingleton<IModuleMiddlewareConfigurator, BlogMiddlewareConfigurator>();

        return services;
    }
}

// --- Optional: Middleware Configurator ---
// Uncomment and implement if Blog needs specific middleware

// public class BlogMiddlewareConfigurator : IModuleMiddlewareConfigurator
// {
//     public IApplicationBuilder Configure(IApplicationBuilder app)
//     {
//         // app.UseMiddleware<SomeBlogMiddleware>();
//         return app;
//     }
// }