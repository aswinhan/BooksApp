using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Discounts.Domain.Policies;
using Modules.Discounts.Features.Features.Shared.Routes;
using Modules.Discounts.Infrastructure.Policies;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.Features.Features.Admin.DeleteCoupon;

public class DeleteCouponEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(DiscountRouteConsts.DeleteCoupon, Handle)
           .RequireAuthorization(DiscountPolicyConsts.ManageDiscountsPolicy).WithName("DeleteCoupon")
           .Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound).WithTags("Discounts.Admin");
    }
    private static async Task<IResult> Handle([FromRoute] Guid couponId, IDeleteCouponHandler h, CancellationToken ct)
    {
        var resp = await h.HandleAsync(couponId, ct); return resp.IsError ? resp.Errors.ToProblem() : Results.NoContent();
    }
}