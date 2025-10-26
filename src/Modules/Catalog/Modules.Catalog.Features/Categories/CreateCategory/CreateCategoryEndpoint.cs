using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Domain.Policies;
using Modules.Catalog.Features.Categories.Shared.Responses;
using Modules.Catalog.Features.Categories.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Categories.CreateCategory;

public class CreateCategoryEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(CategoryRouteConsts.CreateCategory, Handle)
           .RequireAuthorization(CatalogPolicyConsts.ManageCatalogPolicy) // Secure
           .WithName("CreateCategory").Produces<CategoryResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem().ProducesProblem(StatusCodes.Status409Conflict).WithTags("Catalog.Categories");
    }
    private static async Task<IResult> Handle([FromBody] CreateCategoryRequest req, IValidator<CreateCategoryRequest> v, ICreateCategoryHandler h, CancellationToken ct)
    {
        var valResult = await v.ValidateAsync(req, ct); if (!valResult.IsValid) return Results.ValidationProblem(valResult.ToDictionary());
        var resp = await h.HandleAsync(req, ct); if (resp.IsError) return resp.Errors.ToProblem();
        return Results.Created(CategoryRouteConsts.BaseRoute + $"/{resp.Value?.Id}", resp.Value); // Use ID for location
    }
}