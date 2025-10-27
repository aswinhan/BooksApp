using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Required for transaction
using Microsoft.Extensions.Logging;
using Modules.Catalog.PublicApi; // Required to get book details
using Modules.Common.Application.Payments; // Required for IPaymentService
using Modules.Common.Domain.Events; // Required for IEventPublisher
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Discounts.PublicApi; // Required for Discounts API
using Modules.Discounts.PublicApi.Contracts; // Required for DTOs
using Modules.Inventory.PublicApi; // Required for Inventory API
using Modules.Inventory.PublicApi.Contracts; // Required for DTOs
using Modules.Orders.Domain.Abstractions; // Required for ICartService
using Modules.Orders.Domain.Entities; // Required for Order, OrderItem
using Modules.Orders.Domain.Enums; // Required for PaymentMethod
using Modules.Orders.Domain.ValueObjects;
using Modules.Orders.Infrastructure.Database; // Required for OrdersDbContext

namespace Modules.Orders.Features.Checkout;

// Event definition
public record OrderCreatedEvent(Guid OrderId, string UserId) : IEvent;

// Interface definition (Corrected return type)
internal interface ICheckoutHandler : IHandler
{
    Task<Result<(Guid OrderId, string? ClientSecret)>> HandleAsync(string userId, CheckoutRequest request, CancellationToken cancellationToken);
}

// Implementation class
internal sealed class CheckoutHandler(
    ICartService cartService,
    ICatalogModuleApi catalogApi,
    OrdersDbContext dbContext,
    IInventoryModuleApi inventoryApi,
    IDiscountsModuleApi discountsApi,
    IPaymentService paymentService,
    IEventPublisher eventPublisher,
    ILogger<CheckoutHandler> logger)
    : ICheckoutHandler
{
    // Corrected return type
    public async Task<Result<(Guid OrderId, string? ClientSecret)>> HandleAsync(
        string userId, CheckoutRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting checkout for User {UserId} with Payment {PaymentMethod}", userId, request.PaymentMethod);
        var cart = await cartService.GetCartAsync(userId); // Assuming ICartService methods now accept CancellationToken
        if (cart.Items.Count == 0)
        {
            logger.LogWarning("Checkout failed for User {UserId}: Cart is empty.", userId);
            return Error.Validation("Orders.CartEmpty", "Cannot checkout with an empty cart.");
        }

        // --- Payment Intent Logic ---
        string? paymentIntentId = null;
        string? clientSecret = null;
        if (request.PaymentMethod == PaymentMethod.CreditCard)
        {
            long amountInSmallestUnit = (long)(cart.FinalTotal * 100); // Assumes 2 decimal places
            if (amountInSmallestUnit > 0)
            {
                string currency = "inr"; // Or get from config
                logger.LogInformation("Creating/Updating Stripe Payment Intent for amount {Amount}", amountInSmallestUnit);
                var intentResult = await paymentService.CreateOrUpdatePaymentIntentAsync(
                    null, amountInSmallestUnit, currency,
                    new Dictionary<string, string> { { "UserId", userId }, { "CartId", GetCartKey(userId) } }, // Example metadata
                    cancellationToken
                );
                if (intentResult.IsError)
                {
                    logger.LogError("Checkout failed for User {UserId}: Payment Intent creation failed.", userId);
                    return intentResult.Errors!; // Return payment service error
                }
                paymentIntentId = intentResult.Value!.IntentId;
                clientSecret = intentResult.Value!.ClientSecret;
                logger.LogInformation("Stripe Payment Intent {IntentId} created/updated.", paymentIntentId);
            }
            else
            {
                logger.LogInformation("Skipping Payment Intent creation for zero amount order.");
            }
        }
        // --- End Payment Intent ---

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // --- Check Stock ---
            var stockRequestItems = cart.Items.Select(item => new StockAdjustmentItem(item.BookId, item.Quantity)).ToList();
            var stockCheckRequest = new StockAdjustmentRequest(stockRequestItems);
            var stockCheckResult = await inventoryApi.CheckStockAsync(stockCheckRequest, cancellationToken);
            if (stockCheckResult.IsError)
            {
                logger.LogWarning("Checkout failed for User {UserId}: Stock check failed.", userId);
                await transaction.RollbackAsync(cancellationToken);
                return stockCheckResult.Errors!;
            }
            // --- End Stock Check ---

            // --- Determine Billing Address ---
            Address billingAddressToUse;
            if (request.UseShippingAddressForBilling || request.BillingAddress == null)
            {
                billingAddressToUse = request.ShippingAddress; // Use shipping address
            }
            else
            {
                billingAddressToUse = request.BillingAddress; // Use provided billing address
            }
            // --- End Determine Billing ---

            // Create Order
            var order = new Order(Guid.NewGuid(), userId, request.ShippingAddress, billingAddressToUse, request.PaymentMethod);
            if (!string.IsNullOrEmpty(paymentIntentId)) { order.SetPaymentIntentId(paymentIntentId); }

            // --- Set Initial Status ---
            // Keep Pending for Credit Card (wait for webhook confirmation)
            // Keep Pending for Bank Transfer (requires manual confirmation)
            // Move to Processing for COD (assuming stock checked & ready to ship)
            if (request.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                // Use the domain method to ensure rules are checked
                var setResult = order.SetStatusToProcessing();
                if (setResult.IsError)
                {
                    // This shouldn't happen if starting from Pending, but handle defensively
                    logger.LogWarning("Could not set initial status to Processing for COD Order {OrderId}: {Error}", order.Id, setResult.FirstError.Description);
                    await transaction.RollbackAsync(cancellationToken);
                    return setResult.Errors!; // Return the domain error
                }
                logger.LogInformation("Setting initial status to Processing for COD Order {OrderId}", order.Id);
            }
            // else it stays Pending from constructor
            // --- End Set Initial Status ---

            // Add Items
            foreach (var item in cart.Items)
            {
                order.AddOrderItem(item.BookId, item.Title, item.Price, item.Quantity);
            }

            // Apply Coupon Details
            if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                order.ApplyCoupon(cart.AppliedCouponCode, cart.DiscountAmount);
                logger.LogInformation("Applying Coupon {CouponCode} (Discount: {Amount}) to Order {OrderId}",
                    cart.AppliedCouponCode, cart.DiscountAmount, order.Id);
            }

            // --- Set Calculated Amounts from Cart ---
            order.SetCalculatedAmounts(cart.ShippingCost, cart.TaxAmount);
            // --- End Set ---

            // Save Order
            await dbContext.Orders.AddAsync(order, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Decrease Stock
            var decreaseStockRequest = new StockAdjustmentRequest(stockRequestItems);
            var decreaseResult = await inventoryApi.DecreaseStockAsync(decreaseStockRequest, cancellationToken);
            if (decreaseResult.IsError)
            {
                logger.LogError("Checkout failed for User {UserId}: Stock decrease failed after order save. Rolling back.", userId);
                await transaction.RollbackAsync(cancellationToken);
                return decreaseResult.Errors!;
            }

            // Record Coupon Usage
            if (!string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                var recordUsageRequest = new RecordUsageRequest(cart.AppliedCouponCode);
                var recordResult = await discountsApi.RecordCouponUsageAsync(recordUsageRequest, cancellationToken);
                if (recordResult.IsError)
                {
                    logger.LogError("Checkout failed for User {UserId}: Failed to record usage for Coupon {CouponCode}. Rolling back. Error: {@Errors}",
                        userId, cart.AppliedCouponCode, recordResult.Errors);
                    await transaction.RollbackAsync(cancellationToken);
                    return Error.Failure("Orders.CouponUsageRecordFailed", $"Failed to finalize coupon '{cart.AppliedCouponCode}'. Please try again or remove the coupon.");
                }
                logger.LogInformation("Successfully recorded usage for Coupon {CouponCode} for Order {OrderId}",
                    cart.AppliedCouponCode, order.Id);
            }

            // Clear Cart (Pass CancellationToken if service supports it)
            await cartService.ClearCartAsync(userId);

            // Commit Transaction
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Checkout successful for User {UserId}. Order {OrderId} created.", userId, order.Id);

            // Publish Event
            var orderEvent = new OrderCreatedEvent(order.Id, userId);
            await eventPublisher.PublishAsync(orderEvent, cancellationToken);

            // Return success tuple
            return (OrderId: order.Id, ClientSecret: clientSecret);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checkout failed for User {UserId} during transaction.", userId);
            await transaction.RollbackAsync(cancellationToken);
            return Error.Unexpected("Orders.CheckoutFailed", "An unexpected error occurred during checkout.");
        }
    }

    // Helper to get cart key - duplicated from RedisCartService, consider moving to a shared place if needed often
    private static string GetCartKey(string userId) => $"cart:{userId}";
}