using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Catalog.Features.Authors.Shared.Responses;
using Modules.Catalog.Features.Authors.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Authors.GetAuthorsList;

public class GetAuthorsListEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(AuthorRouteConsts.GetAuthorsList, Handle)
           .AllowAnonymous() // Allow anyone to view authors
           .WithName("GetAuthorsList")
           .Produces<List<AuthorResponse>>(StatusCodes.Status200OK)
           .WithTags("Catalog.Authors");
    }

    private static async Task<IResult> Handle(
        IGetAuthorsListHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(cancellationToken);
        // Assuming handler returns Result<List<AuthorResponse>>
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }
        return Results.Ok(response.Value);
    }
}