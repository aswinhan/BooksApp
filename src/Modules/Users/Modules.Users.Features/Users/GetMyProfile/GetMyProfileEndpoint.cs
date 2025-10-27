using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Users.Features.Users.Shared.Routes;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Users.Features.Users.GetMyProfile;

public class GetMyProfileEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(RouteConsts.GetMyProfile, Handle).RequireAuthorization()
           .WithName("GetMyProfile").Produces<GetMyProfileResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Users");
    }
    private static async Task<IResult> Handle(ClaimsPrincipal user, IGetMyProfileHandler handler, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
        var result = await handler.HandleAsync(userId, ct);
        return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
    }
}