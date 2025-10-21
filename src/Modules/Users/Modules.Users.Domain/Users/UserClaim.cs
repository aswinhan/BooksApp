using Microsoft.AspNetCore.Identity;

namespace Modules.Users.Domain.Users;

public class UserClaim : IdentityUserClaim<string> // Use string for PK
{
    public virtual User User { get; set; } = null!; // Navigation property
}