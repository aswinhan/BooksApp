using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Categories.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Categories.UpdateCategory;

internal interface IUpdateCategoryHandler : IHandler { Task<Result<CategoryResponse>> HandleAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken ct); }
internal sealed class UpdateCategoryHandler(CatalogDbContext db, ILogger<UpdateCategoryHandler> l) : IUpdateCategoryHandler
{
    public async Task<Result<CategoryResponse>> HandleAsync(Guid categoryId, UpdateCategoryRequest req, CancellationToken ct)
    {
        l.LogInformation("Updating category {Id}", categoryId);
        var cat = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
        if (cat is null) return Error.NotFound("Catalog.CategoryNotFound", $"Category {categoryId} not found.");
        var slug = req.Slug.ToLowerInvariant().Replace(" ", "-");
        if ((cat.Name != req.Name && await db.Categories.AnyAsync(c => c.Id != categoryId && c.Name == req.Name, ct)) ||
            (cat.Slug != slug && await db.Categories.AnyAsync(c => c.Id != categoryId && c.Slug == slug, ct)))
        {
            l.LogWarning("Category update failed: Name '{Name}' or Slug '{Slug}' already exists.", req.Name, slug);
            return Error.Conflict("Catalog.CategoryExists", $"Category name '{req.Name}' or slug '{slug}' already exists.");
        }
        cat.Update(req.Name, slug); await db.SaveChangesAsync(ct);
        l.LogInformation("Updated category {Id}", categoryId);
        return new CategoryResponse(cat.Id, cat.Name, cat.Slug, cat.CreatedAtUtc, cat.UpdatedAtUtc);
    }
}