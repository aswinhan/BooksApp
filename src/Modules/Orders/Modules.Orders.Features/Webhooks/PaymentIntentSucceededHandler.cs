using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Events; // For IEventPublisher
using Modules.Orders.Domain.Enums; // For OrderStatus
using Modules.Orders.Infrastructure.Database; // For OrdersDbContext
using Stripe; // For Event, PaymentIntent
using System.Threading.Tasks;

namespace Modules.Orders.Features.Webhooks;

// Handler for successful payments
public class PaymentIntentSucceededHandler(OrdersDbContext dbContext, ILogger<PaymentIntentSucceededHandler> logger) : IStripeEventHandler
{
    private readonly OrdersDbContext _dbContext = dbContext;
    private readonly ILogger<PaymentIntentSucceededHandler> _logger = logger;

    public async Task HandleAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            _logger.LogError("Invalid object type received for PaymentIntentSucceeded event: {ObjectType}", stripeEvent.Data.Object?.GetType().Name);
            return; // Or throw
        }

        _logger.LogInformation("Processing successful payment for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

        // Find the corresponding order using PaymentIntentId
        var order = await _dbContext.Orders
                            .FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntent.Id);

        if (order == null)
        {
            _logger.LogError("Order not found for successful PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            // Critical error - should investigate why order wasn't found
            // Could throw an exception here to signal failure back (though Stripe might retry)
            return;
        }

        // --- Update Order Status ---
        // Check current status to avoid processing multiple times
        if (order.Status == OrderStatus.Pending) // Only update if still pending
        {
            // Update status using domain logic if available, or directly
            order.SetStatusToProcessing(); // Assuming this method exists and handles state change
            _logger.LogInformation("Order {OrderId} status updated to Processing due to successful payment.", order.Id);

            // Save the change
            await _dbContext.SaveChangesAsync();

            // Optional: Publish an internal 'OrderPaidEvent'
            // await eventPublisher.PublishAsync(new OrderPaidEvent(order.Id), CancellationToken.None);
        }
        else
        {
            _logger.LogWarning("Order {OrderId} status was already {Status}. Ignoring duplicate PaymentIntentSucceeded event.", order.Id, order.Status);
        }
    }
}