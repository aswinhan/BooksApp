namespace Modules.Users.Features.Users.LoginUser;

// DTO for the login request body
public sealed record LoginUserRequest(string Email, string Password);