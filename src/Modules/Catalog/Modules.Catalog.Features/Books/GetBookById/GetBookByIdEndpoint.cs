using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // For FromRoute
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using System; // For Guid
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.GetBookById;

public class GetBookByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(BookRouteConsts.GetBookById, Handle)
           .AllowAnonymous() // Allow anyone to view book details
           .WithName("GetBookById") // Important for CreatedAtRoute links
           .Produces<BookResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags("Catalog.Books");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId, // Get ID from the route parameter
        IGetBookByIdHandler handler, // Inject the handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(bookId, cancellationToken);

        if (response.IsError)
        {
            // ToProblem converts NotFound error to 404
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 200 OK with BookResponse body)
        return Results.Ok(response.Value);
    }
}