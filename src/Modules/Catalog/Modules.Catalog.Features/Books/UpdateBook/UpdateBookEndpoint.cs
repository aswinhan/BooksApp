using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.UpdateBook;

public class UpdateBookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Use PUT on the specific book ID route
        app.MapPut(BookRouteConsts.GetBookById, Handle) // Reuses GetById route pattern
           .RequireAuthorization() // Add specific admin/manager policy later
           .WithName("UpdateBook")
           .Produces<BookResponse>(StatusCodes.Status200OK) // Return updated book
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound) // Book or Author not found
           .ProducesProblem(StatusCodes.Status409Conflict) // ISBN conflict
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Catalog.Books");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId, // Get ID from route
        [FromBody] UpdateBookRequest request,
        IValidator<UpdateBookRequest> validator,
        IUpdateBookHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(bookId, request, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Handles NotFound, Conflict, etc.
        }

        return Results.Ok(response.Value); // Return 200 OK with updated book
    }
}