using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies;
using Modules.Orders.Domain.Policies; // Use constants
using System;
using System.Collections.Generic;

namespace Modules.Orders.Infrastructure.Policies;

internal sealed class OrdersPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Orders";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        {
            // Requires EITHER Admin/Manager role OR the specific orders:manage claim
            [OrderPolicyConsts.ManageOrdersPolicy] = policy =>
                 policy.RequireAssertion(context =>
                     context.User.IsInRole("Admin") ||
                     context.User.IsInRole("Manager") || // Add Manager role
                     context.User.HasClaim(c => c.Type == OrderPolicyConsts.ManageOrdersPolicy && c.Value == "true")
                 ),

            // Requires EITHER Admin role OR the specific orders:view:all claim
            [OrderPolicyConsts.ViewAllOrdersPolicy] = policy =>
                 policy.RequireAssertion(context =>
                     context.User.IsInRole("Admin") ||
                     context.User.HasClaim(c => c.Type == OrderPolicyConsts.ViewAllOrdersPolicy && c.Value == "true")
                 )
        };
    }
}