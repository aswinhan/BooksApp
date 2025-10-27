using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Results;
using Modules.Discounts.PublicApi;
using Modules.Discounts.PublicApi.Contracts;
using Modules.Orders.Domain.Abstractions; // Needs ICartService
using Modules.Orders.Domain.DTOs; // Needs CartDto, CartItemDto
using StackExchange.Redis; // Needs IDatabase, IConnectionMultiplexer
using System; // Needs Guid, TimeSpan
using System.Linq; // Needs FirstOrDefault
using System.Text.Json; // Needs JsonSerializer
using System.Threading.Tasks; // Needs Task

namespace Modules.Orders.Infrastructure.Services;

public class RedisCartService(IConnectionMultiplexer redis, IDiscountsModuleApi discountsApi, ILogger<RedisCartService> logger) : ICartService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly IDiscountsModuleApi _discountsApi = discountsApi;
    private readonly ILogger<RedisCartService> _logger = logger;
    private readonly TimeSpan _cartExpiry = TimeSpan.FromDays(7); // How long cart persists

    // Helper to get the Redis key for a user's cart
    private static string GetCartKey(string userId) => $"cart:{userId}";

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cartData = await _database.StringGetAsync(GetCartKey(userId));
        CartDto cart;
        if (cartData.IsNullOrEmpty)
        {
            cart = new CartDto();
        }
        else
        {
            cart = JsonSerializer.Deserialize<CartDto>(cartData!) ?? new CartDto();
        }

        cart.DiscountAmount = cart.AppliedCouponCode != null ? cart.DiscountAmount : 0; // Ensure discount is 0 if no code
        decimal taxableAmount = cart.Subtotal - cart.DiscountAmount;
        cart.TaxAmount = CalculateTax(taxableAmount); // Calculate Tax
        cart.ShippingCost = CalculateShippingCost(cart.Subtotal); // Shipping based on subtotal

        return cart;
    }

    public async Task AddItemToCartAsync(string userId, CartItemDto newItem)
    {
        var cart = await GetCartAsync(userId);

        var existingItem = cart.Items.FirstOrDefault(i => i.BookId == newItem.BookId);
        if (existingItem != null)
        {
            // If item exists, increase quantity
            existingItem.Quantity += newItem.Quantity;
        }
        else
        {
            // If item is new, add it to the list
            cart.Items.Add(newItem);
        }

        // Serialize CartDto to JSON and save back to Redis
        var serializedCart = JsonSerializer.Serialize(cart);
        await _database.StringSetAsync(GetCartKey(userId), serializedCart, _cartExpiry);
    }

    public async Task RemoveItemFromCartAsync(string userId, Guid bookId)
    {
        var cart = await GetCartAsync(userId);
        int itemsRemoved = cart.Items.RemoveAll(i => i.BookId == bookId);

        if (itemsRemoved > 0) // Only update if something changed
        {
            if (cart.Items.Count == 0)
            {
                // If cart is now empty, delete the key from Redis
                await ClearCartAsync(userId);
            }
            else
            {
                // Otherwise, save the updated cart
                var serializedCart = JsonSerializer.Serialize(cart);
                await _database.StringSetAsync(GetCartKey(userId), serializedCart, _cartExpiry);
            }
        }
    }

    public async Task UpdateItemQuantityAsync(string userId, Guid bookId, int newQuantity)
    {
        if (newQuantity <= 0)
        {
            // If new quantity is zero or less, remove the item
            await RemoveItemFromCartAsync(userId, bookId);
            return;
        }

        var cart = await GetCartAsync(userId);
        var itemToUpdate = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (itemToUpdate != null)
        {
            itemToUpdate.Quantity = newQuantity;
            var serializedCart = JsonSerializer.Serialize(cart);
            await _database.StringSetAsync(GetCartKey(userId), serializedCart, _cartExpiry);
        }
        // else: Item not found, do nothing or log warning?
    }


    public async Task ClearCartAsync(string userId)
    {
        await _database.KeyDeleteAsync(GetCartKey(userId));
    }

    public async Task<Result<CartDto>> ApplyCouponToCartAsync(string userId, string couponCode)
    {
        var cart = await GetCartAsync(userId);
        if (cart.Items.Count == 0)
        {
            return Error.Validation("Cart.Empty", "Cannot apply coupon to an empty cart.");
        }

        // Call Discounts module to validate and calculate
        var validationRequest = new ValidateCouponRequest(couponCode, cart.Subtotal);
        var validationResult = await _discountsApi.ValidateAndCalculateDiscountAsync(validationRequest, CancellationToken.None); // Consider passing CancellationToken if needed

        if (validationResult.IsError || validationResult.Value == null || !validationResult.Value.IsValid)
        {
            _logger.LogWarning("Failed to apply coupon {CouponCode} for user {UserId}. Validation errors: {@Errors}", couponCode, userId, validationResult.Errors);
            // If validationResult holds domain errors, return them
            if (validationResult.IsError) return validationResult.Errors!;
            // Otherwise, create a generic validation error
            return Error.Validation("Discount.Invalid", $"Coupon code '{couponCode}' is not valid for this cart.");
        }

        // Update cart DTO
        cart.AppliedCouponCode = validationResult.Value.Code;
        cart.DiscountAmount = validationResult.Value.DiscountAmount;

        decimal taxableAmount = cart.Subtotal - cart.DiscountAmount;
        cart.TaxAmount = CalculateTax(taxableAmount); // Recalculate Tax
        cart.ShippingCost = CalculateShippingCost(cart.Subtotal); // Recalculate Shipping

        // Save updated cart back to Redis
        var serializedCart = JsonSerializer.Serialize(cart);
        await _database.StringSetAsync(GetCartKey(userId), serializedCart, _cartExpiry);

        _logger.LogInformation("Applied coupon {CouponCode} to cart for user {UserId}. Discount: {DiscountAmount}", cart.AppliedCouponCode, userId, cart.DiscountAmount);
        return cart; // Return updated cart
    }

    public async Task<Result<CartDto>> RemoveCouponFromCartAsync(string userId)
    {
        var cart = await GetCartAsync(userId); // GetCartAsync now calculates shipping initially

        if (string.IsNullOrEmpty(cart.AppliedCouponCode))
        {
            // Ensure shipping is still correct even if no coupon was removed
            cart.ShippingCost = CalculateShippingCost(cart.Subtotal);
            return cart;
        }

        _logger.LogInformation("Removing coupon {CouponCode} from cart for user {UserId}.", cart.AppliedCouponCode, userId);
        cart.AppliedCouponCode = null;
        cart.DiscountAmount = 0m;

        decimal taxableAmount = cart.Subtotal - cart.DiscountAmount; // Discount is now 0
        cart.TaxAmount = CalculateTax(taxableAmount); // Recalculate Tax
        cart.ShippingCost = CalculateShippingCost(cart.Subtotal); // Recalculate Shipping

        // Save updated cart back to Redis
        var serializedCart = JsonSerializer.Serialize(cart);
        await _database.StringSetAsync(GetCartKey(userId), serializedCart, _cartExpiry);

        return cart; // Return updated cart
    }

    private static decimal CalculateShippingCost(decimal subtotal)
    {
        // Simple rule: $5 flat rate, free if subtotal >= $50
        const decimal shippingRate = 5.00m;
        const decimal freeShippingThreshold = 50.00m;

        return subtotal >= freeShippingThreshold ? 0m : shippingRate;
    }

    // Add Tax calculation helper
    private static decimal CalculateTax(decimal taxableAmount)
    {
        // Simple example: 5% tax on the amount after discount
        const decimal taxRate = 0.05m;
        return Math.Max(0, taxableAmount * taxRate); // Ensure tax isn't negative
    }
}