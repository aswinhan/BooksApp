using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Pagination;
using Modules.Discounts.Domain.Policies;
using Modules.Discounts.Features.Features.Shared.Responses;
using Modules.Discounts.Features.Features.Shared.Routes;
using Modules.Discounts.Infrastructure.Policies;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.Features.Features.Admin.GetCoupons;

public class GetCouponsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(DiscountRouteConsts.GetCoupons, Handle)
           .RequireAuthorization(DiscountPolicyConsts.ManageDiscountsPolicy).WithName("GetCoupons")
           .Produces<PaginatedResponse<CouponResponse>>(StatusCodes.Status200OK).WithTags("Discounts.Admin");
    }
    private static async Task<IResult> Handle(IGetCouponsHandler h, CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var resp = await h.HandleAsync(page, size, ct); return resp.IsError ? resp.Errors.ToProblem() : Results.Ok(resp.Value);
    }
}