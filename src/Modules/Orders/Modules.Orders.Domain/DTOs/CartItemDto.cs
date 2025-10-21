using System;

namespace Modules.Orders.Domain.DTOs;

public class CartItemDto
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = null!; // Denormalized for display
    public decimal Price { get; set; } // Denormalized price per unit
    public int Quantity { get; set; }
    // Optional: Image URL, etc.
}