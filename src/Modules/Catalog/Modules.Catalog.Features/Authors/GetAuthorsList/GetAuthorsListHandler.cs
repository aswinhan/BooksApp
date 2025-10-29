using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Authors.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // Use Result<>

namespace Modules.Catalog.Features.Authors.GetAuthorsList;

internal interface IGetAuthorsListHandler : IHandler
{
    Task<Result<List<AuthorResponse>>> HandleAsync(CancellationToken cancellationToken);
}

internal sealed class GetAuthorsListHandler(
    CatalogDbContext dbContext,
    ILogger<GetAuthorsListHandler> logger)
    : IGetAuthorsListHandler
{
    public async Task<Result<List<AuthorResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving list of authors.");
        try
        {
            var authors = await dbContext.Authors
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .Select(a => new AuthorResponse(
                    a.Id,
                    a.Name,
                    a.Biography,
                    0, // Placeholder BookCount (could calculate with Include(a=>a.Books).Count() but less efficient)
                    0, // Placeholder TotalSales
                    0, // Placeholder TotalReviews
                    a.CreatedAtUtc,
                    a.UpdatedAtUtc
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} authors.", authors.Count);
            return authors; // Implicit conversion
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve authors list.");
            return Error.Unexpected("Catalog.GetAuthorsFailed", "Failed to retrieve authors list.");
        }
    }
}