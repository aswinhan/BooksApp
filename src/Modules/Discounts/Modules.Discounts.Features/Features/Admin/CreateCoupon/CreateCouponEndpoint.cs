using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Discounts.Features.Features.Shared.Responses;
using Modules.Discounts.Features.Features.Shared.Routes;
using Modules.Discounts.Domain.Policies;
using System.Threading;
using System.Threading.Tasks; // Added Policies using
using Modules.Discounts.Domain.Policies;

namespace Modules.Discounts.Features.Features.Admin.CreateCoupon;

public class CreateCouponEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(DiscountRouteConsts.CreateCoupon, Handle)
           .RequireAuthorization(DiscountPolicyConsts.ManageDiscountsPolicy) // Secure
           .WithName("CreateCoupon").Produces<CouponResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem().ProducesProblem(StatusCodes.Status409Conflict).WithTags("Discounts.Admin");
    }
    private static async Task<IResult> Handle([FromBody] CreateCouponRequest req, IValidator<CreateCouponRequest> v, ICreateCouponHandler h, CancellationToken ct)
    {
        var valResult = await v.ValidateAsync(req, ct); if (!valResult.IsValid) return Results.ValidationProblem(valResult.ToDictionary());
        var resp = await h.HandleAsync(req, ct); if (resp.IsError) return resp.Errors.ToProblem();
        return Results.Created(DiscountRouteConsts.AdminBaseRoute + $"/{resp.Value?.Id}", resp.Value); // Use ID
    }
}