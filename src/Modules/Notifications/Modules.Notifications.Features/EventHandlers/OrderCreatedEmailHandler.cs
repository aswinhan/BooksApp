using Microsoft.AspNetCore.Identity; // <-- Add
using Microsoft.Extensions.Logging;
using Modules.Common.Application.Email; // <-- Add
using Modules.Common.Domain.Events; // <-- Add
using Modules.Orders.Features.Checkout; // <-- Add (for OrderCreatedEvent)
using Modules.Users.Domain.Users; // <-- Add
using System; // <-- Add
using System.Threading; // <-- Add
using System.Threading.Tasks; // <-- Add

namespace Modules.Notifications.Features.EventHandlers;

// Implements IEventHandler for the OrderCreatedEvent
internal sealed class OrderCreatedEmailHandler(
    IEmailService emailService,
    UserManager<User> userManager, // Inject UserManager to find the user
    ILogger<OrderCreatedEmailHandler> logger)
    : IEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling OrderCreatedEvent for Order {OrderId}...", @event.OrderId);

        try
        {
            // 1. Find the user
            var user = await userManager.FindByIdAsync(@event.UserId);
            if (user == null || user.Email == null)
            {
                logger.LogError("Cannot send order confirmation: User {UserId} not found or has no email.", @event.UserId);
                return;
            }

            // 2. Craft the email
            string subject = $"Order Confirmation - #{(@event.OrderId.ToString().Split('-')[0])}"; // Short Order ID
            string bodyHtml = $"""
                <h1>Thank you for your order!</h1>
                <p>Hello {user.DisplayName ?? user.UserName},</p>
                <p>We've received your order (ID: {@event.OrderId}) and are getting it ready.</p>
                <p>You can view your order status in your account profile.</p>
                <p>Thanks,</p>
                <p>The BooksApp Team</p>
                """;

            // 3. Send the email
            await emailService.SendEmailAsync(user.Email, subject, bodyHtml, cancellationToken);

            logger.LogInformation("Order confirmation email sent successfully for Order {OrderId} to {Email}.", @event.OrderId, user.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order confirmation email for Order {OrderId}.", @event.OrderId);
            // Do not throw, as the order was already successfully created.
        }
    }
}