using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Categories.DeleteCategory;

internal interface IDeleteCategoryHandler : IHandler { Task<Result<Success>> HandleAsync(Guid categoryId, CancellationToken ct); }
internal sealed class DeleteCategoryHandler(CatalogDbContext db, ILogger<DeleteCategoryHandler> l) : IDeleteCategoryHandler
{
    public async Task<Result<Success>> HandleAsync(Guid categoryId, CancellationToken ct)
    {
        l.LogInformation("Deleting category {Id}", categoryId);
        var cat = await db.Categories.Include(c => c.Books).FirstOrDefaultAsync(c => c.Id == categoryId, ct); // Include books
        if (cat is null) return Error.NotFound("Catalog.CategoryNotFound", $"Category {categoryId} not found.");
        // Rule: Don't delete if books are associated
        if (cat.Books.Count != 0)
        {
            l.LogWarning("Cannot delete category {Id}, it has associated books.", categoryId);
            return Error.Failure("Catalog.CategoryHasBooks", $"Cannot delete category '{cat.Name}' as it has associated books.");
        }
        db.Categories.Remove(cat); await db.SaveChangesAsync(ct);
        l.LogInformation("Deleted category {Id}", categoryId);
        return Result.Success;
    }
}