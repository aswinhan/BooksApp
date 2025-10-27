using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Authors.Shared.Responses; // Use AuthorResponse
using Modules.Catalog.Features.Authors.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Authors.GetAuthorById;

public class GetAuthorByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(AuthorRouteConsts.GetAuthorById, Handle)
           .AllowAnonymous() // Allow anyone to view author details
           .WithName("GetAuthorById")
           .Produces<AuthorResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags("Catalog.Authors");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid authorId, // Get ID from route
        IGetAuthorByIdHandler handler, // Inject handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(authorId, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Handles NotFound
        }

        return Results.Ok(response.Value); // Return AuthorResponse
    }
}