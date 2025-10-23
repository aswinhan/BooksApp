using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Pagination;

namespace Modules.Catalog.Features.Books.GetBooksList;

public class GetBooksListEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(BookRouteConsts.BaseRoute, Handle)
           .AllowAnonymous()
           .WithName("GetBooksList")
           // Update Produces type
           .Produces<PaginatedResponse<BookListResponse>>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status500InternalServerError)
           .WithTags("Catalog.Books");
    }

    private static async Task<IResult> Handle(
    IGetBooksListHandler handler, // Inject the handler
    CancellationToken cancellationToken,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
    {
        // Pass parameters to the handler
        var response = await handler.HandleAsync(pageNumber, pageSize, cancellationToken);


        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }
        return Results.Ok(response.Value); // Return PaginatedResponse
    }
}