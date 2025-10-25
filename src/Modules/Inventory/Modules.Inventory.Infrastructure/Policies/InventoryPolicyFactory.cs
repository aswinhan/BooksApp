using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies;
using System;
using System.Collections.Generic;

namespace Modules.Inventory.Infrastructure.Policies;

internal sealed class InventoryPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Inventory";
    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        // Example: Policy for managing stock (e.g., manual updates)
        // const string ManageStockPolicy = "inventory:manage";
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>()
        {
            // [ManageStockPolicy] = policy => policy.RequireRole("Admin")
        };
    }
}