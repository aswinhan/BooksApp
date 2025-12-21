using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Domain.Users;
using Modules.Users.Features.Users.RegisterUser; // Need the Request DTO
using Modules.Users.Features.Users.Shared.Routes;
using Modules.Users.Infrastructure.Database;
using Xunit;

namespace BooksApp.Tests.Integration.Users;

// This tells xUnit to create one instance of BooksAppApiFactory
// and share it across all tests in this class.
public class UsersApiTests : IClassFixture<BooksAppApiFactory>, IAsyncLifetime
{
    private readonly BooksAppApiFactory _factory;
    private readonly HttpClient _client;

    public UsersApiTests(BooksAppApiFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(); // Create an HttpClient for making API requests
    }

    // --- Test 1: Register User ---
    [Fact]
    public async Task Register_ShouldCreateUser_WhenRequestIsValid()
    {
        // Arrange
        var request = new RegisterUserRequest(
            Email: "testuser@example.com",
            Password: "ValidPassword123!",
            DisplayName: "Test User",
            Role: "User" // Assign "User" role (must be seeded or created)
        );

        // Act
        // Send a POST request to the /api/users/register endpoint
        var response = await _client.PostAsJsonAsync(RouteConsts.Register,request);

        // Assert (Part 1: Check the HTTP Response)
        response.EnsureSuccessStatusCode(); // Throws if status code is not 2xx
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        // Assert (Part 2: Check the Database directly)
        // We create a new scope to get our DbContext
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        // Find the user that should have been created
        var user = await dbContext.Users
                                .FirstOrDefaultAsync(u => u.Email == request.Email);

        // Verify the user exists and data is correct
        user.Should().NotBeNull();
        user!.DisplayName.Should().Be(request.DisplayName);
        user.EmailConfirmed.Should().BeFalse(); // Default
    }

    // This method runs *before each test*
    public async Task InitializeAsync()
    {
        // Clean the database before *each* test runs
        await _factory.ResetDatabaseAsync();

        // --- Seed necessary data ---
        // We must create the "User" role so registration can assign it.
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = "User",
                NormalizedName = "USER"
            });
        }
    }

    // This method runs *after each test*
    public Task DisposeAsync() => Task.CompletedTask;
}