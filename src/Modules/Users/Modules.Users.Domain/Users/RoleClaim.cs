using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.Users;

public class RoleClaim : IdentityRoleClaim<string> // Use string for PK
{
    public virtual Role Role { get; set; } = null!; // Navigation property
}