using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Events;
using Modules.Orders.Domain.Enums;
using Modules.Orders.Infrastructure.Database;
using Stripe;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Webhooks;

// Handler for failed payments
public class PaymentIntentFailedHandler(OrdersDbContext dbContext, ILogger<PaymentIntentFailedHandler> logger) : IStripeEventHandler
{
    private readonly OrdersDbContext _dbContext = dbContext;
    private readonly ILogger<PaymentIntentFailedHandler> _logger = logger;

    public async Task HandleAsync(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            _logger.LogError("Invalid object type received for PaymentIntentFailed event.");
            return;
        }

        _logger.LogWarning("Processing failed payment for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

        var order = await _dbContext.Orders
                            .FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntent.Id);

        if (order == null)
        {
            _logger.LogError("Order not found for failed PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        // --- Update Order Status ---
        if (order.Status == OrderStatus.Pending) // Only update if still pending
        {
            // Set status to Failed
            order.SetStatusToFailed(); // Assuming this method exists
            _logger.LogWarning("Order {OrderId} status updated to Failed due to payment failure.", order.Id);
            await _dbContext.SaveChangesAsync();

            // Optional: Publish OrderPaymentFailedEvent
        }
        else
        {
            _logger.LogWarning("Order {OrderId} status was already {Status}. Ignoring PaymentIntentFailed event.", order.Id, order.Status);
        }
    }
}