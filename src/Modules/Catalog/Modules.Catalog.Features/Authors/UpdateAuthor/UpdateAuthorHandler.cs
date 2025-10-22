using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Authors.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Authors.UpdateAuthor;

internal interface IUpdateAuthorHandler : IHandler
{
    Task<Result<AuthorResponse>> HandleAsync(Guid authorId, UpdateAuthorRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateAuthorHandler(
    CatalogDbContext dbContext,
    ILogger<UpdateAuthorHandler> logger)
    : IUpdateAuthorHandler
{
    public async Task<Result<AuthorResponse>> HandleAsync(Guid authorId, UpdateAuthorRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to update author with ID: {AuthorId}", authorId);

        var author = await dbContext.Authors.FirstOrDefaultAsync(a => a.Id == authorId, cancellationToken);

        if (author is null)
        {
            logger.LogWarning("Update author failed: Author {AuthorId} not found.", authorId);
            return Error.NotFound("Catalog.AuthorNotFound", $"Author with ID {authorId} not found.");
        }

        // Optional: Check for name conflict if name changed
        // if (author.Name != request.Name && await dbContext.Authors.AnyAsync(...)) return Error.Conflict(...);

        // Use domain method if available, or update directly for simple entities
        author.UpdateDetails(request.Name, request.Biography); // Assuming UpdateDetails exists

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated author with ID: {AuthorId}", authorId);

        var response = new AuthorResponse(
            author.Id, author.Name, author.Biography, author.CreatedAtUtc, author.UpdatedAtUtc
        );

        return response;
    }
}