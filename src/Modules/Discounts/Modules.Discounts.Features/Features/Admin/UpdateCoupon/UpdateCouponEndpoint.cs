using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Discounts.Domain.Policies;
using Modules.Discounts.Features.Features.Shared.Responses;
using Modules.Discounts.Features.Features.Shared.Routes;

namespace Modules.Discounts.Features.Features.Admin.UpdateCoupon;

public class UpdateCouponEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(DiscountRouteConsts.UpdateCoupon, Handle)
           .RequireAuthorization(DiscountPolicyConsts.ManageDiscountsPolicy).WithName("UpdateCoupon")
           .Produces<CouponResponse>(StatusCodes.Status200OK).ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound).WithTags("Discounts.Admin");
    }
    private static async Task<IResult> Handle([FromRoute] Guid couponId, [FromBody] UpdateCouponRequest req, IValidator<UpdateCouponRequest> v, IUpdateCouponHandler h, CancellationToken ct)
    {
        var valResult = await v.ValidateAsync(req, ct); if (!valResult.IsValid) return Results.ValidationProblem(valResult.ToDictionary());
        var resp = await h.HandleAsync(couponId, req, ct); return resp.IsError ? resp.Errors.ToProblem() : Results.Ok(resp.Value);
    }
}