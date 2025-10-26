using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Catalog.Features.Categories.Shared.Responses;
using Modules.Catalog.Features.Categories.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Categories.GetCategoriesList;

public class GetCategoriesListEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(CategoryRouteConsts.GetCategoriesList, Handle).AllowAnonymous()
           .WithName("GetCategoriesList").Produces<List<CategoryResponse>>(StatusCodes.Status200OK).WithTags("Catalog.Categories");
    }
    private static async Task<IResult> Handle(IGetCategoriesListHandler h, CancellationToken ct)
    {
        var resp = await h.HandleAsync(ct); return resp.IsError ? resp.Errors.ToProblem() : Results.Ok(resp.Value);
    }
}