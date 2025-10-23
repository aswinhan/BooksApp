using System;
using System.Linq; // Required for mapping in response
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Required for AnyAsync
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities; // Required for Author
using Modules.Catalog.Features.Authors.Shared.Responses; // Required for AuthorResponse
using Modules.Catalog.Infrastructure.Database; // Required for CatalogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // Required for Result<>, Error

namespace Modules.Catalog.Features.Authors.CreateAuthor;

// Interface for the handler
internal interface ICreateAuthorHandler : IHandler
{
    Task<Result<AuthorResponse>> HandleAsync(CreateAuthorRequest request, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class CreateAuthorHandler(
    CatalogDbContext dbContext,
    ILogger<CreateAuthorHandler> logger)
    : ICreateAuthorHandler
{
    public async Task<Result<AuthorResponse>> HandleAsync(CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to create author: {Name}", request.Name);

        // --- Check if author with same name already exists ---
        var nameExists = await dbContext.Authors
                                    .AnyAsync(a => a.Name == request.Name, cancellationToken);
        if (nameExists)
        {
            logger.LogWarning("Create author failed: Name '{Name}' already exists.", request.Name);
            // Return the Conflict error
            return Error.Conflict("Catalog.AuthorNameExists", $"Author with name '{request.Name}' already exists.");
        }
        // --- End of check ---

        // Create the new Author entity
        var author = new Author(
            Guid.NewGuid(),
            request.Name,
            request.Biography
        );

        // Add to DbContext and Save
        await dbContext.Authors.AddAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created author {AuthorId} with Name: {Name}", author.Id, author.Name);

        // Map to Response DTO
        var response = new AuthorResponse(
            author.Id,
            author.Name,
            author.Biography,
            author.CreatedAtUtc,
            author.UpdatedAtUtc
        );

        // Return successful result
        return response; // Implicit conversion
    }
}