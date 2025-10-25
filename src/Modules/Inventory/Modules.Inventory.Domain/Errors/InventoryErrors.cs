using Modules.Common.Domain.Results;
using System;

namespace Modules.Inventory.Domain.Errors;

public static class InventoryErrors
{
    private const string Prefix = "Inventory";

    public static Error BookNotFound(Guid bookId) =>
        Error.NotFound($"{Prefix}.BookNotFound", $"Stock record for Book {bookId} not found.");

    public static Error InsufficientStock(Guid bookId, int required, int available) =>
         Error.Conflict($"{Prefix}.InsufficientStock", $"Insufficient stock for Book {bookId}. Required: {required}, Available: {available}");

    public static Error NegativeQuantity() =>
         Error.Validation($"{Prefix}.NegativeQuantity", "Quantity cannot be negative.");

    public static Error InvalidDecreaseQuantity() =>
        Error.Validation($"{Prefix}.InvalidDecreaseQuantity", "Quantity to decrease must be positive.");

    public static Error InvalidIncreaseQuantity() =>
       Error.Validation($"{Prefix}.InvalidIncreaseQuantity", "Quantity to increase must be positive.");
}