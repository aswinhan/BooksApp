using Modules.Common.Domain; // For IAuditableEntity
using System;

namespace Modules.Orders.Domain.Entities;

public class OrderItem : IAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; } // Foreign key to Order

    // --- Product Snapshot ---
    // Store product info directly on the item
    // This prevents changes in the Catalog affecting past orders
    public Guid BookId { get; private set; } // Link back to the original book
    public string BookTitle { get; private set; } = null!; // Snapshot of the title
    public decimal Price { get; private set; } // Snapshot of the price per unit
    // --- End Product Snapshot ---

    public int Quantity { get; private set; }

    // Navigation property back to Order (optional but useful)
    public Order Order { get; private set; } = null!;

    // Private constructor for EF Core
    private OrderItem() { }

    // Internal constructor - only the Order aggregate should create items
    internal OrderItem(Guid id, Guid orderId, Guid bookId, string bookTitle, decimal price, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Id = id;
        OrderId = orderId;
        BookId = bookId;
        BookTitle = bookTitle;
        Price = price;
        Quantity = quantity;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}