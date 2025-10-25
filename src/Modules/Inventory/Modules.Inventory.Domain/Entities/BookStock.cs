using Modules.Common.Domain; // For IAuditableEntity
using Modules.Common.Domain.Results;
using System;

namespace Modules.Inventory.Domain.Entities;

// Represents the stock level for a specific book
public class BookStock : IAuditableEntity
{
    public Guid Id { get; private set; } // Internal ID for stock record
    public Guid BookId { get; private set; } // Foreign key to Catalog.Book
    public int QuantityAvailable { get; private set; }

    // Private constructor for EF Core
    private BookStock() { }

    // Public constructor
    public BookStock(Guid id, Guid bookId, int initialQuantity)
    {
        if (initialQuantity < 0) throw new ArgumentOutOfRangeException(nameof(initialQuantity), "Initial quantity cannot be negative.");
        Id = id;
        BookId = bookId;
        QuantityAvailable = initialQuantity;
        CreatedAtUtc = DateTime.UtcNow;
    }

    // --- Domain Logic ---
    public Result<Success> DecreaseStock(int quantityToDecrease)
    {
        if (quantityToDecrease <= 0)
        {
            return Error.Validation("Inventory.DecreaseQtyInvalid", "Quantity to decrease must be positive.");
        }
        if (QuantityAvailable < quantityToDecrease)
        {
            return Error.Failure("Inventory.InsufficientStock", $"Insufficient stock for Book {BookId}. Required: {quantityToDecrease}, Available: {QuantityAvailable}");
        }
        QuantityAvailable -= quantityToDecrease;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success;
    }

    public Result<Success> IncreaseStock(int quantityToIncrease)
    {
        if (quantityToIncrease <= 0)
        {
            return Error.Validation("Inventory.IncreaseQtyInvalid", "Quantity to increase must be positive.");
        }
        QuantityAvailable += quantityToIncrease;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success;
    }

    public void SetStock(int newQuantity)
    {
        if (newQuantity < 0) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity cannot be negative.");
        QuantityAvailable = newQuantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }


    // --- IAuditableEntity ---
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}