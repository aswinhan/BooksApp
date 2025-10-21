namespace Modules.Users.Features.Users.RefreshToken;

// DTO for the refresh token request body
public sealed record RefreshTokenRequest(string Token, string RefreshToken);