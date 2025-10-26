using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Books.Shared.Responses; // Use BookListResponse
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Collections.Generic; // For List<>
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.GetRelatedBooks;

public class GetRelatedBooksEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(BookRouteConsts.GetRelatedBooks, Handle)
           .AllowAnonymous() // Allow anyone to see related books
           .WithName("GetRelatedBooks")
           .Produces<List<BookListResponse>>(StatusCodes.Status200OK) // Returns a list
           .ProducesProblem(StatusCodes.Status404NotFound) // If original book not found
           .WithTags("Catalog.Books");
    }

    private static async Task<IResult> Handle(
        IGetRelatedBooksHandler handler, // Inject handler
        [FromRoute] Guid bookId, // Get the ID of the book we're finding related items for
        CancellationToken cancellationToken, // Cancellation token
        [FromQuery] int count = 5// Optional: Limit the number of related books returned
        )
    {
        var response = await handler.HandleAsync(bookId, count, cancellationToken);

        if (response.IsError)
        {
            // Handles NotFound, etc.
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value); // Return List<BookListResponse>
    }
}