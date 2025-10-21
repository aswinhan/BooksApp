namespace Modules.Users.Domain.Policies;

// Defines the string constants for our authorization policies
public static class UserPolicyConsts
{
    public const string ReadPolicy = "users:read";
    public const string CreatePolicy = "users:create";
    public const string UpdatePolicy = "users:update";
    public const string DeletePolicy = "users:delete";
}