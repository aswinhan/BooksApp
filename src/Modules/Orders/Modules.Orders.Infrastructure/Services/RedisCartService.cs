using Modules.Orders.Domain.Abstractions; // Needs ICartService
using Modules.Orders.Domain.DTOs; // Needs CartDto, CartItemDto
using StackExchange.Redis; // Needs IDatabase, IConnectionMultiplexer
using System; // Needs Guid, TimeSpan
using System.Linq; // Needs FirstOrDefault
using System.Text.Json; // Needs JsonSerializer
using System.Threading.Tasks; // Needs Task

namespace Modules.Orders.Infrastructure.Services;

public class RedisCartService(IConnectionMultiplexer redis) : ICartService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly TimeSpan _cartExpiry = TimeSpan.FromDays(7); // How long cart persists

    // Helper to get the Redis key for a user's cart
    private static string GetCartKey(string userId) => $"cart:{userId}";

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cartData = await _database.StringGetAsync(GetCartKey(userId));
        if (cartData.IsNullOrEmpty)
        {
            return new CartDto(); // Return empty cart if not found
        }
        // Deserialize JSON string from Redis back into CartDto
        return JsonSerializer.Deserialize<CartDto>(cartData!) ?? new CartDto();
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
}