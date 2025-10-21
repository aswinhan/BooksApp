namespace Modules.Users.Features.Users.UpdateUser;

// DTO for the update request body
// Properties are nullable to indicate optional updates
public sealed record UpdateUserRequest(string Email, string? DisplayName, string? Role); // Added DisplayName