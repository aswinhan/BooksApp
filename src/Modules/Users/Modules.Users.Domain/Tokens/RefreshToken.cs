using Modules.Common.Domain; // For IAuditableEntity
using Modules.Users.Domain.Users; // For the User navigation property
using System; // For DateTime

namespace Modules.Users.Domain.Tokens;

// Our database entity to store refresh tokens
public class RefreshToken : IAuditableEntity
{
    // Primary Key - The unique token string itself
    public string Token { get; set; } = null!;

    // Links this refresh token to the specific JWT it was issued with
    public string JwtId { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    // Flag to mark if the token has been used or invalidated
    public bool Invalidated { get; set; }

    // Foreign key to the User
    public string UserId { get; set; } = null!;

    // Navigation property back to the User
    public User User { get; set; } = null!;

    // IAuditableEntity properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}