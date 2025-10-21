using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.Users;

// Custom join entity for the many-to-many relationship
public class UserRole : IdentityUserRole<string> // Use string for PKs
{
    // Navigation properties back to User and Role
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}