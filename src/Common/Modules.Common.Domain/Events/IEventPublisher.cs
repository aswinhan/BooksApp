namespace Modules.Common.Domain.Events;

/// <summary>
/// Interface for publishing events to handlers.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <param name="event">The event instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}