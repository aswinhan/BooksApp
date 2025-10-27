using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // For [FromBody], HttpRequest
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.Extensions.DependencyInjection; // For CreateScope
using Microsoft.Extensions.Logging; // For ILogger
using Modules.Common.API.Abstractions;
using Modules.Common.Domain.Events; // For IEventPublisher (optional for internal events)
using Modules.Orders.Features.Shared.Routes;
using Stripe; // For EventUtility, Event
using System;
using System.IO; // For StreamReader
using System.Threading.Tasks;

namespace Modules.Orders.Features.Webhooks;

public class StripeWebhookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(OrderRouteConsts.StripeWebhook, Handle)
           .AllowAnonymous() // Webhooks come from Stripe, not users
           .WithName("StripeWebhook")
           .WithTags("Webhooks"); // Separate tag
    }

    // Stripe requires reading the raw request body, so we inject HttpRequest
    private static async Task<IResult> Handle(
        HttpRequest request,
        IServiceProvider serviceProvider, // To resolve scoped services
        IConfiguration configuration, // To get secrets
        ILogger<StripeWebhookEndpoint> logger) // Logger for the endpoint
    {
        var json = await new StreamReader(request.Body).ReadToEndAsync();
        var stripeSignature = request.Headers["Stripe-Signature"];
        // Get webhook secret from User Secrets / Config
        var webhookSecret = configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            logger.LogError("Stripe Webhook secret is not configured.");
            return Results.BadRequest("Webhook secret configuration error.");
        }

        Event stripeEvent;
        try
        {
            // Validate the event signature
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
            logger.LogInformation("Stripe event received: {EventType}, ID: {EventId}", stripeEvent.Type, stripeEvent.Id);
        }
        catch (StripeException e)
        {
            logger.LogError(e, "Stripe webhook signature validation failed.");
            return Results.BadRequest("Invalid Stripe signature.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing Stripe webhook.");
            return Results.BadRequest("Webhook processing error.");
        }


        // --- Handle the event ---
        // Use a scope because handlers might need scoped services (like DbContext)
        using var scope = serviceProvider.CreateScope();
        var handlerFactory = scope.ServiceProvider.GetRequiredService<StripeEventHandlerFactory>();

        try
        {
            var handler = handlerFactory.GetHandler(stripeEvent.Type);
            if (handler != null)
            {
                await handler.HandleAsync(stripeEvent);
            }
            else
            {
                logger.LogWarning("No handler found for Stripe event type: {EventType}", stripeEvent.Type);
                // Optionally return OK even if unhandled, so Stripe doesn't retry non-critical events
            }
        }
        catch (Exception ex)
        {
            // Log error from handler but return OK to Stripe to prevent retries
            // if the error is temporary or non-critical. Return 500 for critical errors.
            logger.LogError(ex, "Error executing handler for Stripe event {EventId} ({EventType})", stripeEvent.Id, stripeEvent.Type);
            // Consider returning Results.Problem("Handler execution failed") for critical errors
        }


        // Acknowledge receipt to Stripe
        return Results.Ok();
    }
}