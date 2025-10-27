using Modules.Common.Domain;
using Modules.Common.Domain.Results;
using Modules.Orders.Domain.Enums;
using Modules.Orders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Orders.Domain.Entities;

public class Order : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!; // Foreign key to User (string)
    public Address ShippingAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; } // Calculated total

    // Private field for items, exposed as read-only
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // --- ADD Coupon Properties ---
    public string? AppliedCouponCode { get; private set; }
    public decimal DiscountAmount { get; private set; } // Store the calculated discount
    // --- End Coupon ---

    // Private constructor for EF Core
    private Order() { }

    // Public constructor for creating a new Order
    public Order(Guid id, string userId, Address shippingAddress)
    {
        Id = id;
        UserId = userId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending; // Initial status
        Total = 0; // Initial total
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- Aggregate Root Business Logic ---

    /// <summary>
    /// Adds an item to the order.
    /// </summary>
    public void AddOrderItem(Guid bookId, string bookTitle, decimal price, int quantity)
    {
        // Rule: Cannot add items to a finalized order (e.g., Shipped, Cancelled)
        if (Status != OrderStatus.Pending && Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot add items to order with status {Status}.");
        }

        // Optional Rule: Check if item already exists and update quantity?
        var existingItem = _orderItems.FirstOrDefault(i => i.BookId == bookId);
        if (existingItem != null)
        {
            // For now, let's throw, assuming items are added once from cart
            throw new InvalidOperationException($"Book {bookId} is already in the order.");
            // Or: existingItem.UpdateQuantity(existingItem.Quantity + quantity); // Need UpdateQuantity method
        }
        else
        {
            var newItem = new OrderItem(Guid.NewGuid(), this.Id, bookId, bookTitle, price, quantity);
            _orderItems.Add(newItem);
        }


        RecalculateTotal();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // Method to apply coupon details during creation/checkout
    // Call this *after* adding items and *before* final SaveChanges in handler
    public void ApplyCoupon(string code, decimal discountAmount)
    {
        // Can add validation here if needed, but primary validation is in Discounts module
        AppliedCouponCode = code;
        DiscountAmount = Math.Max(0, discountAmount); // Ensure discount isn't negative
        RecalculateTotal(); // Update total after applying discount
        UpdatedAtUtc = DateTime.UtcNow;
    }


    // Example method to update status (implement other transitions similarly)
    public void SetStatusToProcessing()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot set status to Processing from {Status}.");
        }
        Status = OrderStatus.Processing;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    // Add methods for Ship(), Deliver(), Cancel() etc. applying status transition rules

    public Result<Success> Ship() // Use Result<> for domain rules
    {
        // Rule: Can only ship orders that are 'Processing'
        if (Status != OrderStatus.Processing)
        {
            // Return a domain error
            return Error.Validation("Orders.InvalidStatusTransition", $"Cannot ship order with status {Status}.");
        }
        Status = OrderStatus.Shipped;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success; // Indicate success
    }

    public Result<Success> Deliver()
    {
        // Rule: Can only deliver orders that are 'Shipped'
        if (Status != OrderStatus.Shipped)
        {
            return Error.Validation("Orders.InvalidStatusTransition", $"Cannot deliver order with status {Status}.");
        }
        Status = OrderStatus.Delivered;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        // Rule: Cannot cancel orders that are already 'Delivered' or 'Shipped' (adjust as needed)
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Shipped)
        {
            return Error.Validation("Orders.InvalidStatusTransition", $"Cannot cancel order with status {Status}.");
        }
        // Rule: Cannot cancel if already cancelled
        if (Status == OrderStatus.Cancelled)
        {
            return Result.Success; // Idempotent - already cancelled
        }

        Status = OrderStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
        // Consider if stock needs to be increased via an event here
        return Result.Success;
    }


    /// <summary>
    /// Recalculates the total based on current items.
    /// </summary>
    // Update RecalculateTotal to consider discount
    private void RecalculateTotal()
    {
        decimal subtotal = _orderItems.Sum(item => item.Price * item.Quantity);
        // Ensure total doesn't go below zero
        Total = Math.Max(0, subtotal - DiscountAmount);
    }

    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}