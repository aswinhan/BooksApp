using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Orders.Features.Shared.Routes;
using System; // For Guid
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Checkout;

// DTO for the successful checkout response
public record CheckoutResponse(Guid OrderId);

public class CheckoutEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(OrderRouteConsts.Checkout, Handle)
           .RequireAuthorization() // Must be logged in to checkout
           .WithName("Checkout")
           .Produces<CheckoutResponse>(StatusCodes.Status201Created) // Success, returns Order ID
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status400BadRequest) // e.g., Cart empty, Stock issue
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders.Checkout");
    }

    private static async Task<IResult> Handle(
        [FromBody] CheckoutRequest request,
        IValidator<CheckoutRequest> validator,
        ICheckoutHandler handler, // Inject the handler
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
            // ToProblem handles various errors like NotFound, Conflict, Validation, Failure
            return response.Errors.ToProblem();
        }

        // Return 201 Created with the new Order ID
        return Results.Created(OrderRouteConsts.OrderBaseRoute + $"/{response.Value}", // Location header
                               new CheckoutResponse(response.Value)); // Response body
    }
}