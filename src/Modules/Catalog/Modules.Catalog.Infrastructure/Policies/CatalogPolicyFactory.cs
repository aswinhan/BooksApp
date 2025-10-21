using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies; // Use IPolicyFactory
// using Modules.Catalog.Domain.Policies; // We'll define CatalogPolicyConsts later if needed
using System;
using System.Collections.Generic;

namespace Modules.Catalog.Infrastructure.Policies;

internal sealed class CatalogPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Catalog"; // Module identifier

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        // Define policies specific to Catalog here if needed later
        // Example:
        // return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        // {
        //     [CatalogPolicyConsts.ManageProductsPolicy] = policy =>
        //         policy.RequireRole("Admin", "ProductManager"), // Requires Admin OR ProductManager role
        //
        //     [CatalogPolicyConsts.ReadProductsPolicy] = policy =>
        //         policy.RequireAuthenticatedUser() // Any logged-in user can read
        // };

        // For now, return an empty dictionary as we'll use standard policies first
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>();
    }
}