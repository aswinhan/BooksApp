using System; // For DateTime, Guid, etc.
using System.Collections.Generic; // For List, IList
using System.IdentityModel.Tokens.Jwt; // For JwtRegisteredClaimNames, JwtSecurityTokenHandler
using System.Linq; // For SingleOrDefault, FirstOrDefault
using System.Security.Claims; // For ClaimsPrincipal, Claim
using System.Text; // For Encoding
using System.Threading; // For CancellationToken
using System.Threading.Tasks; // For Task
using Microsoft.AspNetCore.Identity; // For UserManager, SignInManager, RoleManager
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync, ToListAsync
using Microsoft.Extensions.Caching.Memory; // For IMemoryCache
using Microsoft.Extensions.Logging; // For ILogger
using Microsoft.Extensions.Options; // For IOptions
using Microsoft.IdentityModel.Tokens; // For TokenValidationParameters, SymmetricSecurityKey, etc.
using Modules.Common.Domain.Results; // For Result<>, Success, Error
using Modules.Common.Infrastructure.Configuration; // For AuthConfiguration
using Modules.Users.Domain.Authentication; // For IClientAuthorizationService, LoginUserResponse, etc.
using Modules.Users.Domain.Errors; // For UserErrors
using Modules.Users.Domain.Tokens; // For RefreshToken, RevocatedTokenType
using Modules.Users.Domain.Users; // For User, Role
using Modules.Users.Infrastructure.Database; // For UsersDbContext

namespace Modules.Users.Infrastructure.Authorization;

public class ClientAuthorizationService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<Role> roleManager,
    ILogger<ClientAuthorizationService> logger,
    IOptions<AuthConfiguration> authOptions, // Use IOptions to get config
    TokenValidationParameters tokenValidationParameters, // Inject pre-configured parameters
    UsersDbContext dbContext,
    IMemoryCache memoryCache) // For token revocation check
    : IClientAuthorizationService
{
    private readonly AuthConfiguration _authConfiguration = authOptions.Value; // Get config value

    // --- Login ---
    public async Task<Result<LoginUserResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting login for email: {Email}", email);
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            logger.LogWarning("Login failed: User not found for email {Email}", email);
            return UserErrors.InvalidCredentials(); // Return specific error
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed: Invalid password for email {Email}", email);
            return UserErrors.InvalidCredentials();
        }

        logger.LogInformation("Login successful for email: {Email}, generating tokens...", email);
        var (token, refreshToken) = await GenerateJwtAndRefreshTokenAsync(user, null, cancellationToken);

        return new LoginUserResponse(token, refreshToken);
    }

    // --- Refresh Token ---
    public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting token refresh.");
        var validatedTokenPrincipal = GetPrincipalFromExpiredToken(token, tokenValidationParameters);
        if (validatedTokenPrincipal is null)
        {
            logger.LogWarning("Refresh token failed: Invalid expired token provided.");
            return UserErrors.InvalidToken();
        }

        // Extract JTI (JWT ID) from the expired token
        var jti = validatedTokenPrincipal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrEmpty(jti))
        {
            logger.LogWarning("Refresh token failed: JTI claim missing from expired token.");
            return UserErrors.InvalidToken();
        }

        // Find the refresh token in the database
        var storedRefreshToken = await dbContext.RefreshTokens
                                        .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (storedRefreshToken is null)
        {
            logger.LogWarning("Refresh token failed: Refresh token not found in database.");
            return UserErrors.InvalidToken();
        }
        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            logger.LogWarning("Refresh token failed: Refresh token expired.");
            dbContext.RefreshTokens.Remove(storedRefreshToken); // Clean up expired token
            await dbContext.SaveChangesAsync(cancellationToken);
            return UserErrors.InvalidToken();
        }
        if (storedRefreshToken.Invalidated)
        {
            logger.LogWarning("Refresh token failed: Refresh token has been invalidated.");
            return UserErrors.InvalidToken();
        }
        if (storedRefreshToken.JwtId != jti) // Ensure refresh token matches the original JWT
        {
            logger.LogWarning("Refresh token failed: JTI mismatch.");
            return UserErrors.InvalidToken();
        }

        // Extract User ID from the expired token's claims
        // Using NameIdentifier which Identity usually maps to UserId
        var userId = validatedTokenPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            logger.LogError("Refresh token failed: UserId (NameIdentifier claim) not found in expired token.");
            return UserErrors.InvalidToken();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            logger.LogError("Refresh token failed: User {UserId} not found.", userId);
            return UserErrors.InvalidToken();
        }

        // Invalidate the used refresh token and generate new ones
        storedRefreshToken.Invalidated = true;
        storedRefreshToken.UpdatedAtUtc = DateTime.UtcNow;
        dbContext.RefreshTokens.Update(storedRefreshToken); // Mark as updated
        // Note: GenerateJwtAndRefreshTokenAsync saves the new refresh token

        logger.LogInformation("Refresh token validated for user {UserId}, generating new tokens...", userId);
        var (newToken, newRefreshToken) = await GenerateJwtAndRefreshTokenAsync(user, storedRefreshToken.Token, cancellationToken); // Pass old token for removal

        return new RefreshTokenResponse(newToken, newRefreshToken);
    }

    // --- Update User Role ---
    public async Task<Result<Success>> UpdateUserRoleAsync(string userId, string newRole, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to update role for user {UserId} to {NewRole}", userId, newRole);

        var roleExists = await roleManager.RoleExistsAsync(newRole);
        if (!roleExists)
        {
            logger.LogWarning("Role update failed: Role '{NewRole}' does not exist.", newRole);
            return UserErrors.RoleNotFound(newRole);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            logger.LogWarning("Role update failed: User {UserId} not found.", userId);
            return UserErrors.NotFound(userId);
        }

        // Transactionally update roles and invalidate tokens
        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Contains(newRole))
            {
                logger.LogInformation("User {UserId} already has role {NewRole}. No update needed.", userId, newRole);
                await transaction.RollbackAsync(cancellationToken); // No changes needed
                return Result.Success;
            }

            if (currentRoles.Any())
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    logger.LogError("Role update failed: Could not remove existing roles for user {UserId}. Errors: {@Errors}", userId, removeResult.Errors);
                    await transaction.RollbackAsync(cancellationToken);
                    return UserErrors.UpdateRoleFailed(removeResult.Errors);
                }
            }

            var addResult = await userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                logger.LogError("Role update failed: Could not add new role {NewRole} for user {UserId}. Errors: {@Errors}", newRole, userId, addResult.Errors);
                await transaction.RollbackAsync(cancellationToken);
                return UserErrors.UpdateRoleFailed(addResult.Errors);
            }

            // Invalidate all active refresh tokens for this user
            var refreshTokens = await dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.Invalidated)
                .ToListAsync(cancellationToken);

            if (refreshTokens.Count != 0)
            {
                logger.LogInformation("Invalidating {Count} refresh tokens for user {UserId} due to role change.", refreshTokens.Count, userId);
                foreach (var refreshToken in refreshTokens)
                {
                    refreshToken.Invalidated = true;
                    refreshToken.UpdatedAtUtc = DateTime.UtcNow;

                    // Add JTI to memory cache for immediate revocation check by middleware
                    // Cache expiration should be longer than JWT expiration
                    memoryCache.Set(refreshToken.JwtId, RevocatedTokenType.RoleChanged, TimeSpan.FromHours(1));
                }
                dbContext.RefreshTokens.UpdateRange(refreshTokens);
                await dbContext.SaveChangesAsync(cancellationToken); // Save token invalidation
            }

            await transaction.CommitAsync(cancellationToken); // Commit role change and token invalidation
            logger.LogInformation("Successfully updated role for user {UserId} to {NewRole} and invalidated tokens.", userId, newRole);
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during role update transaction for user {UserId}.", userId);
            await transaction.RollbackAsync(cancellationToken);
            return Error.Unexpected("Users.RoleUpdateTransaction", "An error occurred during the role update process.");
        }
    }

    // --- Helper: Generate JWT and Refresh Token ---
    private async Task<(string token, string refreshToken)> GenerateJwtAndRefreshTokenAsync(User user, string? oldRefreshTokenToReplace, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "User"; // Default role if none assigned

        var role = await roleManager.FindByNameAsync(userRole);
        IList<Claim> roleClaims = role is not null ? await roleManager.GetClaimsAsync(role) : [];

        var token = GenerateJwtToken(user, _authConfiguration, userRole, roleClaims);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(token, user, oldRefreshTokenToReplace, cancellationToken);

        return (token, refreshToken.Token);
    }

    // --- Helper: Generate JWT ---
    private static string GenerateJwtToken(User user, AuthConfiguration authConfiguration, string userRole, IList<Claim> roleClaims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfiguration.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenId = Guid.NewGuid().ToString(); // Unique ID for this specific token

        // Standard claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email!),        // Subject (usually email or username)
            new(JwtRegisteredClaimNames.Jti, tokenId),            // JWT ID
            new(ClaimTypes.NameIdentifier, user.Id),              // User ID (standard claim type)
            new(ClaimTypes.Email, user.Email!),                   // Email (standard claim type)
            new(ClaimTypes.Role, userRole)                        // User's primary role
            // Add custom claims like DisplayName if needed
            // new("displayName", user.DisplayName ?? string.Empty)
        };

        // Add specific permission claims associated with the user's role
        foreach (var roleClaim in roleClaims)
        {
            // Avoid adding duplicate role claims if already present
            if (!claims.Any(c => c.Type == roleClaim.Type)) // Simple check, adjust if complex logic needed
            {
                claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
            }
        }


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30), // JWT Expiration (e.g., 30 minutes)
            Issuer = authConfiguration.Issuer,
            Audience = authConfiguration.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // --- Helper: Generate and Save Refresh Token ---
    private async Task<RefreshToken> GenerateAndSaveRefreshTokenAsync(string jwtToken, User user, string? oldRefreshTokenToReplace, CancellationToken cancellationToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var parsedJwt = tokenHandler.ReadJwtToken(jwtToken);
        var jti = parsedJwt.Id; // Get the JTI from the JWT

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"), // Generate a new random token string
            JwtId = jti,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token expiration (e.g., 7 days)
            CreatedAtUtc = DateTime.UtcNow,
            Invalidated = false
        };

        // If refreshing, remove the old token being replaced
        if (!string.IsNullOrEmpty(oldRefreshTokenToReplace))
        {
            var tokenToRemove = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldRefreshTokenToReplace, cancellationToken);
            if (tokenToRemove != null)
            {
                dbContext.RefreshTokens.Remove(tokenToRemove);
                logger.LogDebug("Removed old refresh token {OldToken} during refresh for user {UserId}", oldRefreshTokenToReplace, user.Id);
            }
        }


        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken); // Save the new refresh token

        logger.LogDebug("Generated and saved new refresh token for user {UserId}, JWT ID {JwtId}", user.Id, jti);
        return refreshToken;
    }


    // --- Helper: Validate Expired Token Signature ---
    private static ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, TokenValidationParameters parameters)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            // Clone parameters and disable lifetime validation for this check ONLY
            var validationParameters = parameters.Clone();
            validationParameters.ValidateLifetime = false;

            // Validate the token signature and structure (ignoring expiration)
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);

            // Ensure it's a JWT with the expected algorithm
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null; // Invalid algorithm or token type
            }

            return principal;
        }
        catch (Exception) // Catch potential validation errors
        {
            // Log the exception if needed
            //logger.LogWarning(ex, "Failed to validate expired token signature.");
            return null;
        }
    }
}