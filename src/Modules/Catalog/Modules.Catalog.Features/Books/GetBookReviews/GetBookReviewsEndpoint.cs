using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Books.Shared.Responses; // Use ReviewResponse
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Pagination; // Use PaginatedResponse
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.GetBookReviews;

public class GetBookReviewsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Use GET on the reviews sub-resource path
        app.MapGet(BookRouteConsts.GetBookReviews, Handle)
           .AllowAnonymous() // Allow anyone to read reviews
           .WithName("GetBookReviews")
           .Produces<PaginatedResponse<ReviewResponse>>(StatusCodes.Status200OK) // Paginated list
           .ProducesProblem(StatusCodes.Status404NotFound) // If book not found
           .WithTags("Catalog.Books.Reviews"); // Specific tag
    }

    private static async Task<IResult> Handle(
        // Injected services first
        IGetBookReviewsHandler handler,
        // Route parameter next (required)
        [FromRoute] Guid bookId,
        // CancellationToken before optional query parameters
        CancellationToken cancellationToken,
        // Optional query parameters last
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
        )
    {
        // Pass parameters in the correct order to the handler
        var response = await handler.HandleAsync(bookId, pageNumber, pageSize, cancellationToken);

        if (response.IsError)
        {
            // Use ToProblem() for consistency if available, otherwise handle error
            return response.Errors!.ToProblem(); // Use null-forgiving operator if sure Errors isn't null on error
        }

        return Results.Ok(response.Value);
    }
}