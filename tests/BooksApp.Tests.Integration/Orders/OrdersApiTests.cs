using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers; // For AuthenticationHeaderValue
using System.Net.Http.Json;
using System.Security.Claims; // For ClaimsPrincipal
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity; // For RoleManager, UserManager
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Domain.Entities; // For Book, Author, Category
using Modules.Catalog.Infrastructure.Database; // For CatalogDbContext
using Modules.Discounts.Domain.Entities; // For Coupon
using Modules.Discounts.Domain.Enums;
using Modules.Discounts.Infrastructure.Database; // For DiscountsDbContext
using Modules.Inventory.Domain.Entities; // For BookStock
using Modules.Inventory.Infrastructure.Database; // For InventoryDbContext
using Modules.Orders.Domain.Abstractions; // For ICartService
using Modules.Orders.Domain.DTOs; // For CartItemDto
using Modules.Orders.Domain.Enums; // For PaymentMethod
using Modules.Orders.Domain.ValueObjects; // For Address
using Modules.Orders.Features.Checkout; // For CheckoutRequest, CheckoutResponse
using Modules.Orders.Features.Shared.Routes;
using Modules.Orders.Infrastructure.Database;
using Modules.Users.Domain.Authentication; // For IClientAuthorizationService
using Modules.Users.Domain.Users; // For User, Role
namespace BooksApp.Tests.Integration.Orders;

// Use IClassFixture to share the ApiFactory, and IAsyncLifetime for setup/teardown per test
public class OrdersApiTests(BooksAppApiFactory factory) : IClassFixture<BooksAppApiFactory>, IAsyncLifetime
{
    private readonly BooksAppApiFactory _factory = factory;
    private HttpClient _client = null!; // Client for making API requests

    // --- Test Data (seeded before each test) ---
    private User _seededUser = null!;
    private Book _seededBook = null!;
    private Coupon _seededCoupon = null!;
    private string _userAuthToken = null!; // JWT for the seeded user

    // Runs *before* each test method in this class
    public async Task InitializeAsync()
    {
        // 1. Reset the entire database to a clean state
        await _factory.ResetDatabaseAsync();

        // 2. Get a scope to resolve services for seeding
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var inventoryDb = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var discountsDb = scope.ServiceProvider.GetRequiredService<DiscountsDbContext>();
        var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();

        // 3. Seed necessary roles
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new Role { Id = Guid.NewGuid().ToString(), Name = "User", NormalizedName = "USER" });
        }

        // 4. Seed a User
        _seededUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "checkout-user@example.com",
            UserName = "checkout-user@example.com",
            DisplayName = "Checkout User",
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        await userManager.CreateAsync(_seededUser, "ValidPassword123!");
        await userManager.AddToRoleAsync(_seededUser, "User");

        // 5. Seed Catalog (Author, Category, Book)
        var author = new Author(Guid.NewGuid(), "Test Author", null);
        var category = new Category(Guid.NewGuid(), "Test Category", "test-category");
        _seededBook = new Book(Guid.NewGuid(), "Test Book", "A book for testing", "1234567890123", 10.00m, author.Id, category.Id);
        catalogDb.Authors.Add(author);
        catalogDb.Categories.Add(category);
        catalogDb.Books.Add(_seededBook);
        await catalogDb.SaveChangesAsync();

        // 6. Seed Inventory
        var stock = new BookStock(Guid.NewGuid(), _seededBook.Id, 10); // Start with 10 in stock
        inventoryDb.BookStocks.Add(stock);
        await inventoryDb.SaveChangesAsync();

        // 7. Seed Discount
        _seededCoupon = new Coupon(Guid.NewGuid(), "TEST10", DiscountType.FixedAmount, 2.00m, null, 100, 0m);
        discountsDb.Coupons.Add(_seededCoupon);
        await discountsDb.SaveChangesAsync();

        // 8. Seed the Cart in Redis (Add item and apply coupon)
        var cartItem = new CartItemDto { BookId = _seededBook.Id, Title = _seededBook.Title, Price = _seededBook.Price, Quantity = 1 };
        await cartService.AddItemToCartAsync(_seededUser.Id, cartItem);
        await cartService.ApplyCouponToCartAsync(_seededUser.Id, _seededCoupon.Code);

        // 9. Get Auth Token for the User
        // We'll call the service directly to get a token
        var tokenService = scope.ServiceProvider.GetRequiredService<IClientAuthorizationService>();
        var loginResult = await tokenService.LoginAsync(_seededUser.Email!, "ValidPassword123!", CancellationToken.None);
        _userAuthToken = loginResult.Value!.Token;

        // 10. Create an authenticated HttpClient
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userAuthToken);
    }

    // Runs *after* each test
    public Task DisposeAsync() => Task.CompletedTask;


    // --- The Test Method ---
    [Fact]
    public async Task Checkout_ShouldCreateOrder_WhenCartIsValidAndStockExists()
    {
        // Arrange
        var address = new Address("123 Main St", "Test City", "TS", "12345");

        var request = new CheckoutRequest(
            ShippingAddress: address,
            PaymentMethod: PaymentMethod.CashOnDelivery,
            UseShippingAddressForBilling: true,
            BillingAddress: address
        );


        // Act
        var response = await _client.PostAsJsonAsync(OrderRouteConsts.Checkout, request);

        // Assert (Part 1: Check HTTP Response)
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var checkoutResponse = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
        checkoutResponse.Should().NotBeNull();
        checkoutResponse!.OrderId.Should().NotBeEmpty();
        checkoutResponse.ClientSecret.Should().BeNull(); // Null because we used COD

        // Assert (Part 2: Check Database State)
        using var scope = _factory.Services.CreateScope();

        // Verify Order
        var ordersDb = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var order = await ordersDb.Orders
                                  .Include(o => o.OrderItems)
                                  .FirstOrDefaultAsync(o => o.Id == checkoutResponse.OrderId);

        order.Should().NotBeNull();
        order!.UserId.Should().Be(_seededUser.Id);
        order.Status.Should().Be(OrderStatus.Processing); // Changed to Processing because of COD
        order.AppliedCouponCode.Should().Be(_seededCoupon.Code);
        order.DiscountAmount.Should().Be(_seededCoupon.Value);
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().BookId.Should().Be(_seededBook.Id);
        order.OrderItems.First().Price.Should().Be(_seededBook.Price);

        // Verify Inventory
        var inventoryDb = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var stock = await inventoryDb.BookStocks.FirstOrDefaultAsync(b => b.BookId == _seededBook.Id);
        stock.Should().NotBeNull();
        stock!.QuantityAvailable.Should().Be(9); // 10 (initial) - 1 (purchased)

        // Verify Discount Usage
        var discountsDb = scope.ServiceProvider.GetRequiredService<DiscountsDbContext>();
        var coupon = await discountsDb.Coupons.FirstOrDefaultAsync(c => c.Code == _seededCoupon.Code);
        coupon.Should().NotBeNull();
        coupon!.UsageCount.Should().Be(1); // 0 (initial) + 1 (used)

        // Verify Cart is Cleared
        var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
        var cart = await cartService.GetCartAsync(_seededUser.Id);
        cart.Items.Should().BeEmpty();
    }
}