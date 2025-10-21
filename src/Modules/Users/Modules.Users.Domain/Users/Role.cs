using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // For ICollection

namespace Modules.Users.Domain.Users;

// Custom Role class
public class Role : IdentityRole<string> // Use string for PK
{
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RoleClaim> RoleClaims { get; set; } = [];
}