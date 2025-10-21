using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Users;

namespace Modules.Users.Features.Users.DeleteUser;

// Interface for the handler
internal interface IDeleteUserHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string userId, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class DeleteUserHandler(
    UserManager<User> userManager, // Inject UserManager
    ILogger<DeleteUserHandler> logger)
    : IDeleteUserHandler
{
    public async Task<Result<Success>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to delete user with ID: {UserId}", userId);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("Delete failed: User with ID {UserId} not found.", userId);
            // Return NotFound even if delete "succeeds" idempotently
            // Or you could return Success here if you prefer idempotency.
            return UserErrors.NotFound(userId);
        }

        // Delete the user using UserManager
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to delete user {UserId}: {@Errors}", userId, result.Errors);
            return UserErrors.DeleteFailed(result.Errors); // Return specific domain error
        }

        logger.LogInformation("Successfully deleted user with ID: {UserId}", userId);

        // Return success (no value needed)
        return Result.Success;
    }
}