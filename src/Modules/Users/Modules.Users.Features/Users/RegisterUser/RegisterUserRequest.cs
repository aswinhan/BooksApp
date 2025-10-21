namespace Modules.Users.Features.Users.RegisterUser;

// DTO for the registration request body
public sealed record RegisterUserRequest(string Email, string Password, string? DisplayName, string? Role); // Added DisplayName