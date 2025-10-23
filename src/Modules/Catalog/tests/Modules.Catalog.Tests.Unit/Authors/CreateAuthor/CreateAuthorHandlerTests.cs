using System;
using System.Collections.Generic;
using System.Linq; // Keep for potential future use if needed
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions; // For readable assertions
using Microsoft.EntityFrameworkCore; // Required for DbSet, DbContextOptions
using Microsoft.Extensions.Logging;
using Moq; // For creating mocks
using Moq.EntityFrameworkCore; // For mocking DbContext/DbSet
using Modules.Catalog.Domain.Entities; // Required for Author
using Modules.Catalog.Features.Authors.CreateAuthor; // Required for Handler and Request
using Modules.Catalog.Infrastructure.Database; // Required for DbContext
using Xunit; // The testing framework

namespace Modules.Catalog.Tests.Unit.Authors.CreateAuthor;

public class CreateAuthorHandlerTests
{
    // --- Mocks Setup ---
    private readonly Mock<CatalogDbContext> _mockDbContext;
    private readonly Mock<ILogger<CreateAuthorHandler>> _mockLogger;
    private readonly CreateAuthorHandler _handler; // The handler instance under test
    private readonly Mock<DbSet<Author>> _mockAuthorSet;
    private readonly List<Author> _authorStore; // In-memory list simulation

    public CreateAuthorHandlerTests()
    {
        // Initialize Mocks
        _mockDbContext = new Mock<CatalogDbContext>(new DbContextOptions<CatalogDbContext>());
        _mockLogger = new Mock<ILogger<CreateAuthorHandler>>();
        _mockAuthorSet = new Mock<DbSet<Author>>();
        _authorStore = []; // Initialize empty list using collection expression

        // Configure DbContext Mock using Moq.EntityFrameworkCore
        _mockDbContext.Setup(x => x.Authors).ReturnsDbSet(_authorStore, _mockAuthorSet);

        // Instantiate the Handler with mocked dependencies
        _handler = new CreateAuthorHandler(_mockDbContext.Object, _mockLogger.Object);
    }

    // --- Test Method: Success Case ---
    [Fact]
    public async Task HandleAsync_ShouldCreateAuthor_WhenNameIsValidAndDoesNotExist()
    {
        // Arrange
        var request = new CreateAuthorRequest("J.R.R. Tolkien", "Wrote LOTR");
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.HandleAsync(request, cancellationToken);

        // Assert

        // 1. Check Result state
        result.IsSuccess.Should().BeTrue();
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();

        // 2. Check returned AuthorResponse data
        var response = result.Value!;
        response.Name.Should().Be(request.Name);
        response.Biography.Should().Be(request.Biography);
        response.Id.Should().NotBeEmpty(); // Check ID generation
        response.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1)); // Check timestamp (allowing slight difference)
        response.UpdatedAtUtc.Should().BeNull();

        // 3. Verify Mock Interactions
        // Check if AddAsync was called once with the correct Author object
        _mockAuthorSet.Verify(m => m.AddAsync(
            It.Is<Author>(a => a.Name == request.Name && a.Biography == request.Biography),
            cancellationToken),
            Times.Once);

        // Check if SaveChangesAsync was called once
        _mockDbContext.Verify(m => m.SaveChangesAsync(cancellationToken), Times.Once);
    }

    // --- Test Method: Conflict Case ---
    [Fact]
    public async Task HandleAsync_ShouldReturnConflictError_WhenAuthorNameAlreadyExists()
    {
        // Arrange
        var existingAuthorName = "George R.R. Martin";
        // Pre-populate the in-memory store
        _authorStore.Add(new Author(Guid.NewGuid(), existingAuthorName, "Existing Bio"));

        var request = new CreateAuthorRequest(existingAuthorName, "Wrote ASOIAF"); // Duplicate name
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.HandleAsync(request, cancellationToken);

        // Assert

        // 1. Check Result state
        result.IsSuccess.Should().BeFalse();
        result.IsError.Should().BeTrue();
        result.Errors.Should().NotBeNull().And.ContainSingle(); // Check Errors list

        // 2. Check the specific Error
        result.FirstError.Code.Should().Be("Catalog.AuthorNameExists");
        result.FirstError.Description.Should().Contain(existingAuthorName);

        // 3. Verify Mock Interactions
        // Ensure SaveChangesAsync was *not* called in the conflict case
        _mockDbContext.Verify(m => m.SaveChangesAsync(cancellationToken), Times.Never);
        // Ensure AddAsync was *not* called
        _mockAuthorSet.Verify(m => m.AddAsync(It.IsAny<Author>(), cancellationToken), Times.Never);
    }
}