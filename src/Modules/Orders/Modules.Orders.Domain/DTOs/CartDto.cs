using System.Collections.Generic;
using System.Linq;

namespace Modules.Orders.Domain.DTOs;

public class CartDto
{
    // Use dictionary for potentially faster lookups by BookId? Or List is fine.
    public List<CartItemDto> Items { get; set; } = [];

    // Calculated total for the cart
    public decimal Total => Items.Sum(i => i.Price * i.Quantity);
}