using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies;
// using Modules.Orders.Domain.Policies; // Define if needed later
using System;
using System.Collections.Generic;

namespace Modules.Orders.Infrastructure.Policies;

internal sealed class OrdersPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Orders"; // Module identifier

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        // Define policies specific to Orders here if needed later
        // Example:
        // return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        // {
        //     [OrderPolicyConsts.ManageOrdersPolicy] = policy =>
        //         policy.RequireRole("Admin", "OrderManager"),
        //
        //     [OrderPolicyConsts.ViewOwnOrderPolicy] = policy =>
        //         policy.RequireAuthenticatedUser() // Add requirement handler later for owner check
        // };

        // For now, return empty
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>();
    }
}