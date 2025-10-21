namespace Modules.Orders.Domain.Enums;

public enum OrderStatus
{
    Pending,     // Order created, awaiting payment/processing
    Processing,  // Payment received, preparing for shipment
    Shipped,     // Order handed over to carrier
    Delivered,   // Order delivered to customer
    Cancelled,   // Order cancelled
    Failed       // Payment failed or other issue
}