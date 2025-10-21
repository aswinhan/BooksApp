using Microsoft.AspNetCore.Identity; // Needed for IdentityError
using Modules.Common.Domain.Results; // Use our Error record
using System.Collections.Generic;    // Needed for IEnumerable<>
using System.Linq;                 // Needed for Select()

namespace Modules.Users.Domain.Errors;

// Defines specific errors related to user operations
public static class UserErrors
{
    private const string ErrorPrefix = "Users"; // Prefix for error codes

    public static Error NotFound(string userId) =>
        Error.NotFound($"{ErrorPrefix}.NotFound", $"User with ID {userId} not found");

    public static Error NotFoundByEmail(string email) =>
        Error.NotFound($"{ErrorPrefix}.NotFoundByEmail", $"User with email {email} not found");

    public static Error RegistrationFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.Validation($"{ErrorPrefix}.RegistrationFailed", // Use Validation type
           $"Registration failed: {string.Join(", ", identityErrors.Select(e => e.Description))}");

    public static Error UpdateFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.Failure($"{ErrorPrefix}.UpdateFailed", // Use Failure type
           $"User update failed: {string.Join(", ", identityErrors.Select(e => e.Description))}");

    public static Error DeleteFailed(IEnumerable<IdentityError> identityErrors) =>
        Error.Failure($"{ErrorPrefix}.DeleteFailed",
           $"User deletion failed: {string.Join(", ", identityErrors.Select(e => e.Description))}");

    public static Error RefreshFailed() => // Simpler error for refresh
        Error.Unauthorized($"{ErrorPrefix}.RefreshFailed", "Token refresh failed.");

    public static Error RoleNotFound(string roleName) =>
        Error.NotFound($"{ErrorPrefix}.RoleNotFound", $"Role '{roleName}' not found");

    public static Error UpdateRoleFailed(IEnumerable<IdentityError> identityErrors) =>
         Error.Failure($"{ErrorPrefix}.UpdateRoleFailed",
            $"Failed to update role: {string.Join(", ", identityErrors.Select(e => e.Description))}");

    public static Error InvalidCredentials() =>
        Error.Unauthorized($"{ErrorPrefix}.InvalidCredentials", "Invalid email or password"); // Use Unauthorized type

    public static Error InvalidToken() =>
        Error.Unauthorized($"{ErrorPrefix}.InvalidToken", "Invalid token provided"); // Use Unauthorized type
}