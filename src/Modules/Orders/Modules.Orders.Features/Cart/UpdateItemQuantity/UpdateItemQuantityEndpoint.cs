using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Orders.Domain.Abstractions;
using Modules.Orders.Features.Shared.Routes;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Cart.UpdateItemQuantity;

public class UpdateItemQuantityEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Using PUT for updating the quantity of a specific item
        app.MapPut(OrderRouteConsts.UpdateCartItemQuantity, Handle)
           .RequireAuthorization() // Must be logged in
           .WithName("UpdateCartItemQuantity")
           .Produces(StatusCodes.Status204NoContent) // Success, no body
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound) // Item not in cart?
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders.Cart");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId, // Get Book ID from route
        [FromBody] UpdateItemQuantityRequest request,
        IValidator<UpdateItemQuantityRequest> validator,
        ICartService cartService, // Inject service
        ClaimsPrincipal user,
        CancellationToken cancellationToken) // Cancellation token added
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

        // Call the service method directly
        await cartService.UpdateItemQuantityAsync(userId, bookId, request.NewQuantity);

        return Results.NoContent(); // Success
    }
}