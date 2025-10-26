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
            // --- MODIFIED POLICY ---
            // Policy: ManageCatalogPolicy
            // Now requires ONLY the "Admin" role.
            [CatalogPolicyConsts.ManageCatalogPolicy] = policy =>
                policy.RequireRole("Admin") // Directly require the Admin role

            // You could add other policies here later if needed, e.g.:
            // [CatalogPolicyConsts.ReadCatalogPolicy] = policy =>
            //      policy.RequireAuthenticatedUser() // Example: Any logged-in user can read
        };
    }
}