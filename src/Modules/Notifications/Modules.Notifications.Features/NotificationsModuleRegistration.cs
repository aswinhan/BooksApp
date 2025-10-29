using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Extensions;

// ReSharper disable once CheckNamespace
namespace Modules.Notifications.Features;

public static class NotificationsModuleRegistration
{
    // public static string ActivityModuleName => "Notifications"; // Add if tracing needed
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddNotificationsModuleApi().AddNotificationsInfrastructure(configuration);
    }
    private static IServiceCollection AddNotificationsModuleApi(this IServiceCollection services)
    {
        // Register Internal/Public API if needed
        services.RegisterApiEndpointsFromAssemblyContaining(typeof(NotificationsModuleRegistration));
        services.RegisterHandlersFromAssemblyContaining(typeof(NotificationsModuleRegistration));
        services.AddValidatorsFromAssembly(typeof(NotificationsModuleRegistration).Assembly, includeInternalTypes: true);
        // services.AddSingleton<IModuleMiddlewareConfigurator, NotificationsMiddlewareConfigurator>();
        return services;
    }
}
// Add Infrastructure DI extension
public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register IEmailService implementation here later
        return services;
    }
}
// Add Middleware Configurator if needed