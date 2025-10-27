using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Discounts.Features.Features.Shared.Responses;
using Modules.Discounts.Features.Features.Shared.Routes;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.Features.Features.ApplyCoupon;

public class ApplyCouponEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(DiscountRouteConsts.ApplyCoupon, Handle)
           .AllowAnonymous() // Allow anyone to try applying a coupon
                             // Or .RequireAuthorization() if only logged-in users can use coupons
           .WithName("ApplyCoupon")
           .Produces<AppliedCouponResponse>(StatusCodes.Status200OK) // Success
           .ProducesValidationProblem() // Request validation
           .ProducesProblem(StatusCodes.Status404NotFound) // Coupon not found/inactive
           .ProducesProblem(StatusCodes.Status400BadRequest) // Coupon validation failed (e.g., min amount)
           .WithTags("Discounts");
    }

    private static async Task<IResult> Handle(
        [FromBody] ApplyCouponRequest request,
        IValidator<ApplyCouponRequest> validator,
        IApplyCouponHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(request, cancellationToken);

        if (response.IsError)
        {
            // Handles NotFound, Validation errors from Coupon.Validate
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value); // Return AppliedCouponResponse
    }
}