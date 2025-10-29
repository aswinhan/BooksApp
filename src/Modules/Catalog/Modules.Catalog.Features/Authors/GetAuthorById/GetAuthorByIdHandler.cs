using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Authors.Shared.Responses; // Use AuthorResponse
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.PublicApi;
using Modules.Orders.PublicApi.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Authors.GetAuthorById;

internal interface IGetAuthorByIdHandler : IHandler
{
    Task<Result<AuthorResponse>> HandleAsync(Guid authorId, CancellationToken cancellationToken);
}

internal sealed class GetAuthorByIdHandler(
    CatalogDbContext dbContext,
    IOrdersModuleApi ordersApi,
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

        // --- Fetch Stats ---

        // 1. Get Book Count
        int bookCount = await dbContext.Books
                            .AsNoTracking()
                            .CountAsync(b => b.AuthorId == authorId, cancellationToken);

        // 2. Get Review Count (for all books by this author)
        int reviewCount = await dbContext.Reviews
                            .AsNoTracking()
                            .CountAsync(r => r.Book.AuthorId == authorId, cancellationToken);

        // 3. Get Sales Count (Cross-Module Call)
        int salesCount = 0;
        var authorBookIds = await dbContext.Books
                                .Where(b => b.AuthorId == authorId)
                                .Select(b => b.Id)
                                .ToListAsync(cancellationToken);

        if (authorBookIds.Count > 0)
        {
            var salesResult = await ordersApi.GetSalesCountForBooksAsync(
                new GetSalesCountForBooksRequest(authorBookIds), cancellationToken);

            if (salesResult.IsSuccess)
            {
                salesCount = salesResult.Value;
            }
            else
            {
                logger.LogWarning("Could not retrieve sales count for Author {AuthorId}: {Error}",
                    authorId, salesResult.FirstError.Code);
            }
        }
        // --- End Fetch Stats ---

        // Map to response DTO
        var response = new AuthorResponse(
            author.Id,
            author.Name,
            author.Biography,
            bookCount,
            salesCount,
            reviewCount,
            author.CreatedAtUtc,
            author.UpdatedAtUtc
        );

        logger.LogInformation("Successfully retrieved author with ID: {AuthorId}", authorId);
        return response;
    }
}