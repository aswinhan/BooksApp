using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities;
using Modules.Catalog.Features.Authors.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Authors.CreateAuthor;

internal interface ICreateAuthorHandler : IHandler
{
    Task<Result<AuthorResponse>> HandleAsync(CreateAuthorRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateAuthorHandler(
    CatalogDbContext dbContext,
    ILogger<CreateAuthorHandler> logger)
    : ICreateAuthorHandler
{
    public async Task<Result<AuthorResponse>> HandleAsync(CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to create author: {Name}", request.Name);

        // Optional: Check if author with same name already exists
        // var nameExists = await dbContext.Authors.AnyAsync(a => a.Name == request.Name, cancellationToken);
        // if (nameExists) return Error.Conflict("Catalog.AuthorNameExists", ...);

        var author = new Author(
            Guid.NewGuid(),
            request.Name,
            request.Biography
        );

        await dbContext.Authors.AddAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created author {AuthorId} with Name: {Name}", author.Id, author.Name);

        var response = new AuthorResponse(
            author.Id, author.Name, author.Biography, author.CreatedAtUtc, author.UpdatedAtUtc
        );

        return response;
    }
}