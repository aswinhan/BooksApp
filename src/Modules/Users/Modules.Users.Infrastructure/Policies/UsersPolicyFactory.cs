using Microsoft.AspNetCore.Authorization; // For AuthorizationPolicyBuilder
using Modules.Common.Infrastructure.Policies; // Use IPolicyFactory
using Modules.Users.Domain.Policies; // Use UserPolicyConsts
using System; // For Action<>
using System.Collections.Generic; // For Dictionary

namespace Modules.Users.Infrastructure.Policies;

internal sealed class UsersPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Users"; // Identify the module

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        {
            // Policy requiring the "users:read" claim
            [UserPolicyConsts.ReadPolicy] = policy => policy.RequireClaim(UserPolicyConsts.ReadPolicy, "true"),

            // Policy requiring the "users:create" claim
            [UserPolicyConsts.CreatePolicy] = policy => policy.RequireClaim(UserPolicyConsts.CreatePolicy, "true"),

            // Policy requiring the "users:update" claim
            [UserPolicyConsts.UpdatePolicy] = policy => policy.RequireClaim(UserPolicyConsts.UpdatePolicy, "true"),

            // Policy requiring the "users:delete" claim
            [UserPolicyConsts.DeletePolicy] = policy => policy.RequireClaim(UserPolicyConsts.DeletePolicy, "true")
        };
        // Note: We check for claim value "true" for added security.
    }
}