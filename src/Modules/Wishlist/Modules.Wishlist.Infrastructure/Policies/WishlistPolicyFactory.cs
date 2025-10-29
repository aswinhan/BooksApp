using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies;
// using Modules.Wishlist.Domain.Policies; // Define if needed
using System;
using System.Collections.Generic;

namespace Modules.Wishlist.Infrastructure.Policies;

internal sealed class WishlistPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Wishlist";

    // Define policy constants if needed (e.g., ManageAllWishlists for admins)
    // public const string ManageWishlistsPolicy = "wishlist:manage";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        // Define policies specific to Wishlist here if needed later
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>();
    }
}