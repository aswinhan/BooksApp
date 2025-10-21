using Modules.Common.Domain.Results; // Use our Result pattern

namespace Modules.Users.Domain.Authentication;

// Defines the core features this module provides for authentication
public interface IClientAuthorizationService
{
    Task<Result<LoginUserResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken);

    Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);

    Task<Result<Success>> UpdateUserRoleAsync(string userId, string newRole, CancellationToken cancellationToken);
}