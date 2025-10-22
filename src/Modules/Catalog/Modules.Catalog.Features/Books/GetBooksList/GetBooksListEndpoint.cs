using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http; // For Results, IResult
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.GetBooksList;

public class GetBooksListEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(BookRouteConsts.BaseRoute, Handle)
           .AllowAnonymous()
           .WithName("GetBooksList")
           .Produces<List<BookListResponse>>(StatusCodes.Status200OK)
           // Add ProducesProblem if the handler can return specific errors
           .ProducesProblem(StatusCodes.Status500InternalServerError) // For unexpected errors
           .WithTags("Catalog.Books");
    }

    private static async Task<IResult> Handle(
        IGetBooksListHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new GetBooksListQuery();
        var response = await handler.HandleAsync(query, cancellationToken); // Handler now returns Result<>

        // Check if the handler returned an error
        if (response.IsError)
        {
            // Use ToProblem() to convert the error to an HTTP response
            return response.Errors.ToProblem();
        }

        // Return HTTP 200 OK with the list from the Result's Value
        return Results.Ok(response.Value);
    }
}