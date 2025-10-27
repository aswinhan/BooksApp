namespace Modules.Orders.Domain.Enums;

public enum PaymentMethod
{
    Undefined, // Default
    CashOnDelivery,
    BankTransfer,
    CreditCard // Add if integrating Stripe/etc. later
}