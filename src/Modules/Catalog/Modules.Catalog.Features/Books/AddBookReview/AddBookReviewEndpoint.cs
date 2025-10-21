using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Books.Shared.Responses; // For ReviewResponse maybe? Or just NoContent
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using System;
using System.Security.Claims; // To get UserId
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.AddBookReview;

public class AddBookReviewEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(BookRouteConsts.AddReview, Handle)
           .RequireAuthorization() // User must be logged in to review
           .WithName("AddBookReview")
           .Produces(StatusCodes.Status204NoContent) // Success, no body needed
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound) // Book not found
           .ProducesProblem(StatusCodes.Status409Conflict) // User already reviewed
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Catalog.Books");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId, // Get Book ID from route
        [FromBody] AddBookReviewRequest request,
        IValidator<AddBookReviewRequest> validator,
        IAddBookReviewHandler handler,
        ClaimsPrincipal user, // Inject ClaimsPrincipal to get User ID
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // Get UserId from the JWT token's claims
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            // Should not happen if RequireAuthorization is working, but good practice
            return Results.Unauthorized();
        }

        var response = await handler.HandleAsync(bookId, userId, request, cancellationToken);

        if (response.IsError)
        {
            // ToProblem handles NotFound, Conflict, Validation errors
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 204 No Content)
        return Results.NoContent();
    }
}