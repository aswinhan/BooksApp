using Microsoft.Extensions.DependencyInjection; // For IServiceProvider
using Microsoft.Extensions.Logging;
using Stripe; // <--- Ensure this is present
using System;
using System.Collections.Generic; // For Dictionary

namespace Modules.Orders.Features.Webhooks;

// Factory to resolve the correct handler based on event type
public class StripeEventHandlerFactory(IServiceProvider serviceProvider, ILogger<StripeEventHandlerFactory> logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<StripeEventHandlerFactory> _logger = logger;

    // Map Stripe event types (strings) to our handler types
    // USE the Stripe.Events constants here for better type safety
    private static readonly Dictionary<string, Type> _handlerMappings = new()
    {
        { "payment_intent.succeeded", typeof(PaymentIntentSucceededHandler) },
        { "payment_intent.payment_failed", typeof(PaymentIntentFailedHandler) }
        // Add mappings for other events like "charge.refunded", etc.
    };

    public IStripeEventHandler? GetHandler(string eventType)
    {
        if (_handlerMappings.TryGetValue(eventType, out var handlerType))
        {
            // Resolve the specific handler instance from DI container
            try
            {
                // Ensure the resolved service implements the interface
                if (_serviceProvider.GetRequiredService(handlerType) is not IStripeEventHandler handler)
                {
                    _logger.LogError("Resolved service for type {HandlerType} does not implement IStripeEventHandler.", handlerType.Name);
                    return null;
                }
                return handler;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve Stripe event handler for type {HandlerType}", handlerType.Name);
                return null; // Or throw if resolution must succeed
            }
        }
        _logger.LogDebug("No handler registered for Stripe event type: {EventType}", eventType);
        return null; // No handler registered for this event type
    }
}