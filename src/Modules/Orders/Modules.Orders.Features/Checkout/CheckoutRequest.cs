using Modules.Orders.Domain.Enums;
using Modules.Orders.Domain.ValueObjects; // Required for Address

namespace Modules.Orders.Features.Checkout;

// DTO for the checkout request body
// We only need the shipping address from the user during checkout
public record CheckoutRequest(
    Address ShippingAddress,
    PaymentMethod PaymentMethod
// Add PaymentInfo DTO here later if integrating payment gateway
);