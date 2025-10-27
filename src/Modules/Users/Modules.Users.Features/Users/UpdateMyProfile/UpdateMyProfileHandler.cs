using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Users;
using Modules.Users.Features.Users.GetMyProfile; // Reuse response DTO

namespace Modules.Users.Features.Users.UpdateMyProfile;

internal interface IUpdateMyProfileHandler : IHandler
{
    Task<Result<GetMyProfileResponse>> HandleAsync(string userId, UpdateMyProfileRequest request, CancellationToken ct);
}

internal sealed class UpdateMyProfileHandler(
    UserManager<User> userManager,
    ILogger<UpdateMyProfileHandler> logger) : IUpdateMyProfileHandler
{
    public async Task<Result<GetMyProfileResponse>> HandleAsync(string userId, UpdateMyProfileRequest request, CancellationToken ct)
    {
        logger.LogInformation("Updating profile for User {UserId}", userId);
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Update profile failed: User {UserId} not found.", userId);
            return UserErrors.NotFound(userId);
        }

        // Update properties
        user.DisplayName = request.DisplayName; // Assuming non-nullable based on DTO
        user.Street = request.Street ?? user.Street; // Keep existing if null
        user.City = request.City ?? user.City;
        user.State = request.State ?? user.State;
        user.ZipCode = request.ZipCode ?? user.ZipCode;
        user.UpdatedAtUtc = DateTime.UtcNow; // Update audit field

        // Save changes using UserManager
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to update profile for User {UserId}: {@Errors}", userId, result.Errors);
            return UserErrors.UpdateFailed(result.Errors);
        }

        logger.LogInformation("Successfully updated profile for User {UserId}", userId);

        // Map updated user to response DTO
        var response = new GetMyProfileResponse(
            user.Id, user.Email!, user.DisplayName, user.Street, user.City, user.State, user.ZipCode
        );
        return response;
    }
}