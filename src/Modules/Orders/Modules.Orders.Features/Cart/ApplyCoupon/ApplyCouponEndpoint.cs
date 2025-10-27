using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Orders.Domain.Abstractions;
using Modules.Orders.Domain.DTOs;
using Modules.Orders.Features.Shared.Routes;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Cart.ApplyCoupon;

public class ApplyCouponEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(OrderRouteConsts.ApplyCoupon, Handle)
           .RequireAuthorization().WithName("ApplyCartCoupon")
           .Produces<CartDto>(StatusCodes.Status200OK) // Return updated cart
           .ProducesValidationProblem().ProducesProblem(StatusCodes.Status400BadRequest)
           .WithTags("Orders.Cart");
    }
    private static async Task<IResult> Handle(
        [FromBody] ApplyCouponRequestDto req, IValidator<ApplyCouponRequestDto> v,
        ICartService cartSvc, ClaimsPrincipal user, CancellationToken ct)
    {
        var valResult = await v.ValidateAsync(req, ct); if (!valResult.IsValid) return Results.ValidationProblem(valResult.ToDictionary());
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier); if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
        var result = await cartSvc.ApplyCouponToCartAsync(userId, req.CouponCode); // Call service
        return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
    }
}