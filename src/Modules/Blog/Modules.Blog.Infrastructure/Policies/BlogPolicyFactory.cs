using Microsoft.AspNetCore.Authorization;
using Modules.Common.Infrastructure.Policies;
// using Modules.Blog.Domain.Policies; // Define if needed later
using System;
using System.Collections.Generic;

namespace Modules.Blog.Infrastructure.Policies;

internal sealed class BlogPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Blog"; // Module identifier

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        // Define policies specific to Blog here if needed later
        // Example:
        // return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        // {
        //     [BlogPostPolicyConsts.ManagePostsPolicy] = policy =>
        //         policy.RequireRole("Admin", "Editor"),
        // };

        // For now, return empty
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>();
    }
}