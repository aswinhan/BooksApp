using Microsoft.AspNetCore.Identity;
using Modules.Common.Domain; // For IAuditableEntity
using System; // For DateTime
using System.Collections.Generic; // For ICollection

namespace Modules.Users.Domain.Users;

// Our custom User class inheriting from IdentityUser
// We use 'string' as the key type to match the base Identity classes easily
public class User : IdentityUser<string>, IAuditableEntity // Use string for PK
{
    // Navigation properties to related Identity tables
    public virtual ICollection<UserClaim> Claims { get; set; } = [];
    public virtual ICollection<UserLogin> Logins { get; set; } = []; // Renamed from UserLogins for convention
    public virtual ICollection<UserToken> Tokens { get; set; } = []; // Renamed from UserTokens for convention
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    // IAuditableEntity properties
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Add our custom DisplayName property from original plan
    public string? DisplayName { get; set; }

    // --- ADD Address Properties ---
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; } 
    // --- End Address ---
}