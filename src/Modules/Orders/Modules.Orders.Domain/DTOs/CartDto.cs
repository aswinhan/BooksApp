using System.Collections.Generic;
using System.Linq;

namespace Modules.Orders.Domain.DTOs;

public class CartDto
{
    // Use dictionary for potentially faster lookups by BookId? Or List is fine.
    public List<CartItemDto> Items { get; set; } = [];
    public string? AppliedCouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }

    public decimal TaxAmount { get; set; }

    // Calculated subtotal before discount
    public decimal Subtotal => Items.Sum(i => i.Price * i.Quantity);

    // Final total includes subtotal, discount, shipping and tax
    public decimal FinalTotal => (Subtotal - DiscountAmount) + ShippingCost + TaxAmount;
}