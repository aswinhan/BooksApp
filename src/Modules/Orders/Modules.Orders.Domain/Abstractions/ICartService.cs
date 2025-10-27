using Modules.Common.Domain.Results;
using Modules.Orders.Domain.DTOs;
using System;
using System.Threading.Tasks;

namespace Modules.Orders.Domain.Abstractions;

/// <summary>
/// Defines operations for managing a user's shopping cart.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Gets the user's current cart.
    /// </summary>
    Task<CartDto> GetCartAsync(string userId);

    /// <summary>
    /// Adds an item to the user's cart or updates quantity if it exists.
    /// </summary>
    Task AddItemToCartAsync(string userId, CartItemDto item);

    /// <summary>
    /// Removes an item completely from the user's cart.
    /// </summary>
    Task RemoveItemFromCartAsync(string userId, Guid bookId);

    /// <summary>
    /// Updates the quantity of a specific item in the cart.
    /// </summary>
    Task UpdateItemQuantityAsync(string userId, Guid bookId, int newQuantity);

    /// <summary>
    /// Clears all items from the user's cart.
    /// </summary>
    Task ClearCartAsync(string userId);

    /// <summary>
    /// Validates and applies a coupon code to the user's cart.
    /// Returns the updated cart or validation errors.
    /// </summary>
    Task<Result<CartDto>> ApplyCouponToCartAsync(string userId, string couponCode);

    /// <summary>
    /// Removes any applied coupon from the user's cart.
    /// </summary>
    Task<Result<CartDto>> RemoveCouponFromCartAsync(string userId);
}