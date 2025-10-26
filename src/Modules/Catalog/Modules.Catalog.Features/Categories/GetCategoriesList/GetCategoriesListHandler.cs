using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Categories.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Categories.GetCategoriesList;

internal interface IGetCategoriesListHandler : IHandler { Task<Result<List<CategoryResponse>>> HandleAsync(CancellationToken ct); }
internal sealed class GetCategoriesListHandler(CatalogDbContext db, ILogger<GetCategoriesListHandler> l) : IGetCategoriesListHandler
{
    public async Task<Result<List<CategoryResponse>>> HandleAsync(CancellationToken ct)
    {
        l.LogInformation("Retrieving categories list."); try
        {
            var cats = await db.Categories.AsNoTracking().OrderBy(c => c.Name)
                .Select(c => new CategoryResponse(c.Id, c.Name, c.Slug, c.CreatedAtUtc, c.UpdatedAtUtc)).ToListAsync(ct);
            l.LogInformation("Retrieved {Count} categories.", cats.Count); return cats;
        }
        catch (Exception ex) { l.LogError(ex, "Failed retrieving categories."); return Error.Unexpected("Catalog.GetCategoriesFailed", "Failed."); }
    }
}