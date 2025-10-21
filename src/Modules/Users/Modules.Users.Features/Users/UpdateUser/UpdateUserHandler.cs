using System.Linq; // Required for Any()
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For UserManager, RoleManager
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Users;
using Modules.Users.Features.Users.Shared;

namespace Modules.Users.Features.Users.UpdateUser;

// Interface for the handler
internal interface IUpdateUserHandler : IHandler
{
    Task<Result<UserResponse>> HandleAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class UpdateUserHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager, // Inject RoleManager for role updates
    ILogger<UpdateUserHandler> logger)
    : IUpdateUserHandler
{
    public async Task<Result<UserResponse>> HandleAsync(
        string userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to update user with ID: {UserId}", userId);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("Update failed: User with ID {UserId} not found.", userId);
            return UserErrors.NotFound(userId);
        }

        // Update user properties
        user.Email = request.Email;
        user.UserName = request.Email; // Keep UserName synced with Email usually
        user.DisplayName = request.DisplayName ?? user.DisplayName; // Update if provided
        user.UpdatedAtUtc = DateTime.UtcNow; // Update audit field

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            logger.LogError("Failed to update user {UserId}: {@Errors}", userId, updateResult.Errors);
            return UserErrors.UpdateFailed(updateResult.Errors);
        }

        // Update role if a new one is provided
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var roleExists = await roleManager.RoleExistsAsync(request.Role);
            if (!roleExists)
            {
                logger.LogWarning("Role update skipped for user {UserId}: Role '{Role}' does not exist.", userId, request.Role);
                // Decide: return error or just skip role update? Skipping for now.
                // return UserErrors.RoleNotFound(request.Role);
            }
            else
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(request.Role)) // Only update if role is different
                {
                    if (currentRoles.Any()) // Remove existing roles first
                    {
                        var removeRolesResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (!removeRolesResult.Succeeded)
                        {
                            logger.LogError("Failed to remove existing roles for user {UserId}: {@Errors}", userId, removeRolesResult.Errors);
                            // Decide: Fail entire operation or just log? Failing for consistency.
                            return UserErrors.UpdateRoleFailed(removeRolesResult.Errors);
                        }
                    }

                    // Add the new role
                    var addRoleResult = await userManager.AddToRoleAsync(user, request.Role);
                    if (!addRoleResult.Succeeded)
                    {
                        logger.LogError("Failed to add role '{Role}' for user {UserId}: {@Errors}", request.Role, userId, addRoleResult.Errors);
                        // Decide: Fail entire operation or just log? Failing for consistency.
                        return UserErrors.UpdateRoleFailed(addRoleResult.Errors);
                    }
                    logger.LogInformation("Updated role for user {UserId} to {Role}", userId, request.Role);
                }
            }
        }

        logger.LogInformation("Successfully updated user with ID: {UserId}", userId);

        return new UserResponse(user.Id, user.Email!);
    }
}