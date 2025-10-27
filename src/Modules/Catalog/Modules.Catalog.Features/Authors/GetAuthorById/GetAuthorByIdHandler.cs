using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Authors.Shared.Responses; // Use AuthorResponse
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Authors.GetAuthorById;

internal interface IGetAuthorByIdHandler : IHandler
{
    Task<Result<AuthorResponse>> HandleAsync(Guid authorId, CancellationToken cancellationToken);
}

internal sealed class GetAuthorByIdHandler(
    CatalogDbContext dbContext,
    ILogger<GetAuthorByIdHandler> logger)
    : IGetAuthorByIdHandler
{
    public async Task<Result<AuthorResponse>> HandleAsync(Guid authorId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving author with ID: {AuthorId}", authorId);

        var author = await dbContext.Authors
                               .AsNoTracking()
                               .FirstOrDefaultAsync(a => a.Id == authorId, cancellationToken);

        if (author is null)
        {
            logger.LogWarning("Author with ID {AuthorId} not found.", authorId);
            return Error.NotFound("Catalog.AuthorNotFound", $"Author with ID {authorId} not found.");
        }

        // Map to response DTO
        var response = new AuthorResponse(
            author.Id,
            author.Name,
            author.Biography,
            author.CreatedAtUtc,
            author.UpdatedAtUtc
        );

        logger.LogInformation("Successfully retrieved author with ID: {AuthorId}", authorId);
        return response;
    }
}