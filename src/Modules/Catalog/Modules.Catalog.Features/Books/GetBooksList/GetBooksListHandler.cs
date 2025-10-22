using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Books.Shared.Responses; // Use BookListResponse
using Modules.Catalog.Infrastructure.Database; // Use CatalogDbContext
using Modules.Common.Domain.Handlers; // Use IHandler
using Modules.Common.Domain.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// We will return the list directly for now, not Result<>
// using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Books.GetBooksList;

// Interface defines the contract
internal interface IGetBooksListHandler : IHandler
{
    // Adjusting return type for simplicity now, refactor later
    Task<Result<List<BookListResponse>>> HandleAsync(GetBooksListQuery query, CancellationToken cancellationToken);
}

// Implementation handles the query
internal sealed class GetBooksListHandler(
    CatalogDbContext dbContext, // Inject DbContext for reading
    ILogger<GetBooksListHandler> logger)
    : IGetBooksListHandler
{
    // Adjusting return type
    public async Task<Result<List<BookListResponse>>> HandleAsync(GetBooksListQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving list of books.");

        try // Optional: Add try/catch for unexpected database errors
        {
            var books = await dbContext.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .OrderBy(b => b.Title)
                .Select(b => new BookListResponse(
                    b.Id,
                    b.Title,
                    b.Author.Name,
                    b.Price,
                    null
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} books.", books.Count);

            // Wrap the successful result
            return books; // Implicit conversion from List<T> to Result<List<T>> works
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve books list.");
            // Return an unexpected error
            return Error.Unexpected("Catalog.GetListFailed", "Failed to retrieve book list.");
        }
    }
}