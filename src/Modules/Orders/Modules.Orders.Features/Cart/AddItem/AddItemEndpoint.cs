using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using Modules.Orders.Features.Shared.Routes;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Cart.AddItem;

public class AddItemEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(OrderRouteConsts.AddCartItem, Handle)
           .RequireAuthorization() // Must be logged in
           .WithName("AddCartItem")
           .Produces(StatusCodes.Status204NoContent) // Success, no body
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound) // If book doesn't exist (handled in handler)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders.Cart");
    }

    private static async Task<IResult> Handle(
        [FromBody] AddItemRequest request,
        IValidator<AddItemRequest> validator,
        IAddItemHandler handler, // Inject the handler
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var response = await handler.HandleAsync(userId, request, cancellationToken);

        if (response.IsError)
        {
            // ToProblem handles NotFound (for Book)
            return response.Errors.ToProblem();
        }

        return Results.NoContent(); // Success
    }
}