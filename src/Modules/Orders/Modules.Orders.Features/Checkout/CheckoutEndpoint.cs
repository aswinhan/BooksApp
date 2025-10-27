using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
// using Modules.Common.API.Extensions; // Namespace changed for extension methods
using Modules.Orders.Features.Shared.Routes;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Modules.Common.API.Extensions;

namespace Modules.Orders.Features.Checkout;

// Response DTO definition
public record CheckoutResponse(Guid OrderId, string? ClientSecret);

public class CheckoutEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(OrderRouteConsts.Checkout, Handle)
           .RequireAuthorization()
           .WithName("Checkout")
           .Produces<CheckoutResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status400BadRequest)
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

        // Handler now returns Result<(Guid, string?)>
        var response = await handler.HandleAsync(userId, request, cancellationToken);

        if (response.IsError)
        {
            // Assuming ToProblem() extension exists on List<Error>
            return response.Errors!.ToProblem(); // Use null-forgiving operator
        }

        // --- FIX: Access tuple members directly ---
        // Since IsError is false, response.Value is guaranteed not null here
        Guid orderId = response.Value.OrderId; // Access OrderId from the tuple
        string? clientSecret = response.Value.ClientSecret; // Access ClientSecret from the tuple
        // --- End FIX ---

        // Return 201 Created with OrderId and ClientSecret
        return Results.Created(OrderRouteConsts.OrderBaseRoute + $"/{orderId}", // Location header
                               new CheckoutResponse(orderId, clientSecret)); // Response body
    }
}