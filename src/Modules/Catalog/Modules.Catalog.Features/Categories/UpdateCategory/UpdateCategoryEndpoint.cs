using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Domain.Policies;
using Modules.Catalog.Features.Categories.Shared.Responses;
using Modules.Catalog.Features.Categories.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Categories.UpdateCategory;

public class UpdateCategoryEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(CategoryRouteConsts.UpdateCategory, Handle)
           .RequireAuthorization(CatalogPolicyConsts.ManageCatalogPolicy).WithName("UpdateCategory")
           .Produces<CategoryResponse>(StatusCodes.Status200OK).ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict).WithTags("Catalog.Categories");
    }
    private static async Task<IResult> Handle([FromRoute] Guid categoryId, [FromBody] UpdateCategoryRequest req, IValidator<UpdateCategoryRequest> v, IUpdateCategoryHandler h, CancellationToken ct)
    {
        var valResult = await v.ValidateAsync(req, ct); if (!valResult.IsValid) return Results.ValidationProblem(valResult.ToDictionary());
        var resp = await h.HandleAsync(categoryId, req, ct); return resp.IsError ? resp.Errors.ToProblem() : Results.Ok(resp.Value);
    }
}