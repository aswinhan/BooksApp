using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities;
using Modules.Catalog.Features.Categories.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Categories.CreateCategory;

internal interface ICreateCategoryHandler : IHandler { Task<Result<CategoryResponse>> HandleAsync(CreateCategoryRequest request, CancellationToken ct); }
internal sealed class CreateCategoryHandler(CatalogDbContext db, ILogger<CreateCategoryHandler> l) : ICreateCategoryHandler
{
    public async Task<Result<CategoryResponse>> HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        l.LogInformation("Creating category: {Name}", req.Name);
        var slug = req.Slug.ToLowerInvariant().Replace(" ", "-"); // Sanitize slug
        if (await db.Categories.AnyAsync(c => c.Slug == slug || c.Name == req.Name, ct))
        {
            l.LogWarning("Category creation failed: Name '{Name}' or Slug '{Slug}' already exists.", req.Name, slug);
            return Error.Conflict("Catalog.CategoryExists", $"Category with name '{req.Name}' or slug '{slug}' already exists.");
        }
        var cat = new Category(Guid.NewGuid(), req.Name, slug);
        await db.Categories.AddAsync(cat, ct); await db.SaveChangesAsync(ct);
        l.LogInformation("Created category {Id}", cat.Id);
        return new CategoryResponse(cat.Id, cat.Name, cat.Slug, cat.CreatedAtUtc, cat.UpdatedAtUtc);
    }
}