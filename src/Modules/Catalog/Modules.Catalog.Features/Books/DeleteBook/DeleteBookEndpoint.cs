using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.DeleteBook;

public class DeleteBookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Use DELETE on the specific book ID route
        app.MapDelete(BookRouteConsts.GetBookById, Handle) // Reuses GetById route pattern
           .RequireAuthorization() // Add specific admin/manager policy later
           .WithName("DeleteBook")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound) // Book not found
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Catalog.Books");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId, // Get ID from route
        IDeleteBookHandler handler, // Inject handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(bookId, cancellationToken);

        if (response.IsError)
        {
            // ToProblem handles NotFound, etc.
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 204 No Content)
        return Results.NoContent();
    }
}