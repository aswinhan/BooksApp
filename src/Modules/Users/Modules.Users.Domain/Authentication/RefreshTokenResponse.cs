namespace Modules.Users.Domain.Authentication;

// DTO for successful token refresh
public record RefreshTokenResponse(string Token, string RefreshToken);