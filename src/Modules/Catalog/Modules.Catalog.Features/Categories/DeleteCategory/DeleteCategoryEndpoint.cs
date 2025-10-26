using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Domain.Policies;
using Modules.Catalog.Features.Categories.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Categories.DeleteCategory;

public class DeleteCategoryEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(CategoryRouteConsts.DeleteCategory, Handle)
           .RequireAuthorization(CatalogPolicyConsts.ManageCatalogPolicy).WithName("DeleteCategory")
           .Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status400BadRequest).WithTags("Catalog.Categories"); // 400 if books associated
    }
    private static async Task<IResult> Handle([FromRoute] Guid categoryId, IDeleteCategoryHandler h, CancellationToken ct)
    {
        var resp = await h.HandleAsync(categoryId, ct); return resp.IsError ? resp.Errors.ToProblem() : Results.NoContent();
    }
}