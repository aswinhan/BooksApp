using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies;
using Modules.Discounts.Domain.Policies; // <-- ADD THIS USING
using System;
using System.Collections.Generic;

namespace Modules.Discounts.Infrastructure.Policies;

internal sealed class DiscountsPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Discounts";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        {
            // Use the constant from the Domain project now
            [DiscountPolicyConsts.ManageDiscountsPolicy] = policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim(c => c.Type == DiscountPolicyConsts.ManageDiscountsPolicy && c.Value == "true")
                )
        };
    }
}