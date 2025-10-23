using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies;
using Modules.Catalog.Domain.Policies; // Use constants
using System;
using System.Collections.Generic;

namespace Modules.Catalog.Infrastructure.Policies;

internal sealed class CatalogPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Catalog";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        {
            // Policy: ManageCatalogPolicy
            // Requires EITHER the "Admin" role OR the specific "catalog:manage" claim
            [CatalogPolicyConsts.ManageCatalogPolicy] = policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") || // Check for Admin role
                    context.User.HasClaim(c => c.Type == CatalogPolicyConsts.ManageCatalogPolicy && c.Value == "true") // Check for specific claim
                )
            // Alternative: Just require the claim
            // policy.RequireClaim(CatalogPolicyConsts.ManageCatalogPolicy, "true")

            // Add Read policy if needed (e.g., policy.RequireAuthenticatedUser())
        };
    }
}