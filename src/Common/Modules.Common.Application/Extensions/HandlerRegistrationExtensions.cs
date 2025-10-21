using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Domain.Events;
using Modules.Common.Domain.Handlers;

namespace Modules.Common.Application.Extensions;

public static class HandlerRegistrationExtensions
{
    /// <summary>
    /// Registers handlers (IHandler, IEventHandler) from the specified assembly.
    /// </summary>
    public static IServiceCollection RegisterHandlersFromAssemblyContaining(this IServiceCollection services, Type marker)
    {
        var assembly = marker.Assembly;

        // Register command/query handlers (implement IHandler but not IEventHandler)
        RegisterCommandHandlers(services, assembly);

        // Register event handlers (implement IEventHandler)
        RegisterEventHandlers(services, assembly);

        return services;
    }

    private static void RegisterCommandHandlers(IServiceCollection services, Assembly assembly)
    {
        // Find types that are classes, not abstract, implement IHandler, but DO NOT implement IEventHandler
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.IsAssignableTo(typeof(IHandler))
                        && !t.IsAssignableTo(typeof(IEventHandler))) // Exclude event handlers
            .ToList();

        foreach (var implementationType in handlerTypes)
        {
            // Find the specific handler interface (e.g., ICreateUserHandler) it implements
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i != typeof(IHandler) && i.IsAssignableTo(typeof(IHandler)));

            if (interfaceType is not null)
            {
                // Register as scoped (one instance per web request)
                services.AddScoped(interfaceType, implementationType);
            }
        }
    }

    private static void RegisterEventHandlers(IServiceCollection services, Assembly assembly)
    {
        // Find types that are classes, not abstract, and implement IEventHandler
        var eventHandlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.IsAssignableTo(typeof(IEventHandler)))
            .ToList();

        foreach (var implementationType in eventHandlerTypes)
        {
            // Find all the specific IEventHandler<TEvent> interfaces it implements
            var handlerInterfaces = implementationType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

            foreach (var interfaceType in handlerInterfaces)
            {
                // Register the handler for each specific event type it handles
                services.AddScoped(interfaceType, implementationType);
            }
        }
    }
}