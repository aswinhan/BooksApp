using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Application.Pagination;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Discounts.Features.Features.Shared.Responses;
using Modules.Discounts.Infrastructure.Database;

namespace Modules.Discounts.Features.Features.Admin.GetCoupons;

internal interface IGetCouponsHandler : IHandler { Task<Result<PaginatedResponse<CouponResponse>>> HandleAsync(int page, int size, CancellationToken ct); }
internal sealed class GetCouponsHandler(DiscountsDbContext db, ILogger<GetCouponsHandler> l) : IGetCouponsHandler
{
    public async Task<Result<PaginatedResponse<CouponResponse>>> HandleAsync(int page, int size, CancellationToken ct)
    {
        l.LogInformation("Getting coupons list: Page {Page}, Size {Size}", page, size); page = Math.Max(1, page); size = Math.Clamp(size, 5, 100); try
        {
            var query = db.Coupons.AsNoTracking().OrderByDescending(c => c.CreatedAtUtc);
            int total = await query.CountAsync(ct);
            var coupons = await query.Skip((page - 1) * size).Take(size)
                .Select(c => new CouponResponse(c.Id, c.Code, c.Type, c.Value, c.ExpiryDate, c.UsageLimit, c.UsageCount, c.MinimumCartAmount, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc))
                .ToListAsync(ct);
            l.LogInformation("Retrieved {Count} coupons.", coupons.Count);
            return new PaginatedResponse<CouponResponse>(coupons, total, page, size);
        }
        catch (Exception ex) { l.LogError(ex, "Failed retrieving coupons."); return Error.Unexpected("Discount.GetListFailed", "Failed."); }
    }
}