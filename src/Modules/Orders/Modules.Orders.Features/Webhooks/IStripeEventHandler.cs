using Stripe; // Need Event
using System.Threading.Tasks;

namespace Modules.Orders.Features.Webhooks;

// Interface for specific event handlers
public interface IStripeEventHandler
{
    Task HandleAsync(Event stripeEvent);
}