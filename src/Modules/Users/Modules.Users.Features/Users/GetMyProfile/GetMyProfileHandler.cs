using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Users;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Users.Features.Users.GetMyProfile;

internal interface IGetMyProfileHandler : IHandler { Task<Result<GetMyProfileResponse>> HandleAsync(string userId, CancellationToken ct); }
internal sealed class GetMyProfileHandler(UserManager<User> userManager, ILogger<GetMyProfileHandler> logger) : IGetMyProfileHandler
{
    public async Task<Result<GetMyProfileResponse>> HandleAsync(string userId, CancellationToken ct)
    {
        logger.LogDebug("Getting profile for User {UserId}", userId);
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return UserErrors.NotFound(userId);
        var response = new GetMyProfileResponse(user.Id, user.Email!, user.DisplayName, user.Street, user.City, user.State, user.ZipCode);
        return response;
    }
}