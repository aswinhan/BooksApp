using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For UserManager
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors; // For UserErrors
using Modules.Users.Domain.Users; // For User
using Modules.Users.Features.Users.Shared; // For UserResponse
// Note: We don't directly use DbContext here, UserManager abstracts it

namespace Modules.Users.Features.Users.GetUserById;

// Interface for the handler
internal interface IGetUserByIdHandler : IHandler
{
    Task<Result<UserResponse>> HandleAsync(string userId, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class GetUserByIdHandler(
    UserManager<User> userManager, // Use UserManager to find users
    ILogger<GetUserByIdHandler> logger)
    : IGetUserByIdHandler
{
    public async Task<Result<UserResponse>> HandleAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to retrieve user with ID: {UserId}", userId);

        // Find the user using UserManager
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("User with ID {UserId} not found.", userId);
            return UserErrors.NotFound(userId); // Return specific domain error
        }

        logger.LogInformation("Successfully retrieved user with ID: {UserId}", userId);

        // Map to the response DTO
        return new UserResponse(user.Id, user.Email!); // Assuming Email is never null for found user
    }
}