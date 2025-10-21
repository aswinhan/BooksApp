namespace Modules.Users.Features.Users.Shared;

// Standard DTO for returning basic user information
public sealed record UserResponse(string Id, string Email);