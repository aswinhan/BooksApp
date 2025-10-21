namespace Modules.Common.Domain.Events;

/// <summary>
/// Base marker interface for all event handlers.
/// </summary>
public interface IEventHandler
{
    // Marker interface
}

/// <summary>
/// Interface for handlers that process a specific event type.
/// </summary>
/// <typeparam name="TEvent">The type of event handled.</typeparam>
public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IEvent
{
    /// <summary>
    /// Handles the specified event.
    /// </summary>
    /// <param name="event">The event instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}