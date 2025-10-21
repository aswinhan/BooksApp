using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Events;
using System; // Added for AggregateException, Exception
using System.Linq; // Added for Select, ToArray
using System.Threading; // Added for CancellationToken
using System.Threading.Tasks; // Added for Task

namespace Modules.Common.Application;

/// <summary>
/// Dispatches events to all registered handlers.
/// </summary>
public class EventPublisher(IServiceProvider serviceProvider, ILogger<EventPublisher> logger)
    : IEventPublisher
{
    /// <summary>
    /// Publishes an event to all relevant handlers.
    /// </summary>
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        var eventType = @event.GetType();
        logger.LogDebug("Publishing event {EventType}", eventType.Name);

        try
        {
            // Get all registered handlers for this specific event type (TEvent)
            var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>().ToArray();

            if (handlers.Length == 0)
            {
                logger.LogDebug("No handlers registered for event {EventType}", eventType.Name);
                return;
            }

            logger.LogDebug("Found {HandlerCount} handlers for event {EventType}", handlers.Length, eventType.Name);

            // Execute all handlers concurrently
            var handlerTasks = handlers
                .Select(handler => ExecuteHandlerAsync(handler, @event, cancellationToken))
                .ToList();

            await Task.WhenAll(handlerTasks);

            // Check if any handler threw an exception
            var exceptions = handlerTasks
                .Select(t => t.Exception?.InnerException) // Get the actual exception
                .Where(ex => ex != null)
                .ToList();

            if (exceptions.Count > 0)
            {
                logger.LogError("One or more handlers failed while processing event {EventType}", eventType.Name);
                // Throw a single exception containing all handler exceptions
                throw new AggregateException($"Errors occurred while handling event {eventType.Name}", exceptions!);
            }

            logger.LogDebug("Successfully published event {EventType}", eventType.Name);
        }
        catch (AggregateException ex)
        {
            // Log the aggregate exception details but rethrow it
            logger.LogError(ex, "AggregateException occurred during event publishing for {EventType}", eventType.Name);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred publishing event {EventType}", eventType.Name);
            throw; // Rethrow unexpected exceptions
        }
    }

    // Helper method to execute a single handler safely
    private async Task<Exception?> ExecuteHandlerAsync<TEvent>(
        IEventHandler<TEvent> handler,
        TEvent @event,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        try
        {
            await handler.HandleAsync(@event, cancellationToken);
            return null; // Success
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing handler {HandlerType} for event {EventType}",
                handler.GetType().Name, @event.GetType().Name);
            return ex; // Return the exception
        }
    }
}