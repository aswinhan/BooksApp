using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Blog.Features.Decorators;
using Modules.Blog.Features.Posts.GetPostBySlug;
using Modules.Blog.Infrastructure;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Caching;
using Modules.Common.Application.Extensions;
using Modules.Common.Domain.Events;
using Modules.Common.Domain.Handlers;

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
        //services.RegisterHandlersFromAssemblyContaining(typeof(BlogModuleRegistration));

        // --- Handler Registration with Caching ---

        // 1. Register the REAL GetPostBySlug handler
        services.AddScoped<GetPostBySlugHandler>();

        // 2. Register the INTERFACE using the decorator factory
        services.AddScoped<IGetPostBySlugHandler>(provider =>
            new CachingGetPostBySlugHandlerDecorator(
                provider.GetRequiredService<GetPostBySlugHandler>(), // Get real handler
                provider.GetRequiredService<ICachingService>()   // Get caching service
            ));

        // 3. Register OTHER handlers directly
        var handlerTypesToRegisterDirectly = AssemblyReference.Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.IsAssignableTo(typeof(IHandler))
                        && !t.IsAssignableTo(typeof(IEventHandler))
                        && t != typeof(GetPostBySlugHandler) // Exclude decorated handler
                        && t != typeof(CachingGetPostBySlugHandlerDecorator) // Exclude decorator
                        )
            .ToList();

        foreach (var implementationType in handlerTypesToRegisterDirectly)
        {
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i != typeof(IHandler)
                                   && i.IsAssignableTo(typeof(IHandler)));
            if (interfaceType is not null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }
        // --- End of Handler Registration ---

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