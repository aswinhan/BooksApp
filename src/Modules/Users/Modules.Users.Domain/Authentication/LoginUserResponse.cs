namespace Modules.Users.Domain.Authentication;

// DTO for successful login, containing both tokens
public sealed record LoginUserResponse(string Token, string RefreshToken);