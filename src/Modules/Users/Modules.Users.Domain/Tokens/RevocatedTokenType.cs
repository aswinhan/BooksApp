namespace Modules.Users.Domain.Tokens;

/// <summary>
/// Specifies the reason why a token was revoked.
/// Used by the CheckRevocatedTokensMiddleware.
/// </summary>
public enum RevocatedTokenType
{
    /// <summary>
    /// Token manually invalidated (e.g., user logged out everywhere).
    /// </summary>
    Invalidated,

    /// <summary>
    /// Token invalidated because the user's role changed.
    /// </summary>
    RoleChanged
}