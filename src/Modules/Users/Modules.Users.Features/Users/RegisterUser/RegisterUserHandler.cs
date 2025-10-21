using System; // For Guid
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For UserManager
using Microsoft.Extensions.Logging; // For ILogger
using Modules.Common.Domain.Handlers; // For IHandler
using Modules.Common.Domain.Results; // For Result<>, Success, Error
using Modules.Users.Domain.Errors; // For UserErrors
using Modules.Users.Domain.Users; // For User
using Modules.Users.Features.Users.Shared; // For UserResponse

namespace Modules.Users.Features.Users.RegisterUser;

// Interface for the handler (dependency inversion)
internal interface IRegisterUserHandler : IHandler
{
    Task<Result<UserResponse>> HandleAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class RegisterUserHandler(
    UserManager<User> userManager, // Inject Identity's UserManager
    ILogger<RegisterUserHandler> logger)
    : IRegisterUserHandler
{
    public async Task<Result<UserResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        // Check if user already exists (added this check)
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Registration failed: Email {Email} already exists.", request.Email);
            return Error.Conflict("Users.EmailExists", $"Email {request.Email} is already registered.");
        }


        var user = new User
        {
            Id = Guid.NewGuid().ToString(), // Generate string ID
            Email = request.Email,
            UserName = request.Email, // Often UserName is the same as Email
            DisplayName = request.DisplayName, // Set display name
            CreatedAtUtc = DateTime.UtcNow // Set audit field
        };

        // Create the user (hashes password automatically)
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to register user {Email}: {@Errors}", request.Email, result.Errors);
            return UserErrors.RegistrationFailed(result.Errors); // Return specific domain error
        }

        // Add role if specified
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            logger.LogInformation("Adding role '{Role}' to user {Email}", request.Role, request.Email);
            var roleResult = await userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                // Log warning but potentially don't fail the whole registration? Or return error?
                // For now, log and continue, but return error might be better.
                logger.LogWarning("Failed to add role '{Role}' to user {Email}: {@Errors}", request.Role, request.Email, roleResult.Errors);
                // Optionally: return UserErrors.UpdateRoleFailed(roleResult.Errors);
            }
        }

        logger.LogInformation("Successfully registered user {Email} with ID: {UserId}", user.Email, user.Id);

        // Return success with the UserResponse DTO
        return new UserResponse(user.Id, user.Email);
    }
}