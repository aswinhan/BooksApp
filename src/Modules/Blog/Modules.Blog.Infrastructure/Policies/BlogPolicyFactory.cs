using Microsoft.AspNetCore.Authorization;
using Modules.Blog.Domain.Policies; // Use constants
using Modules.Common.Infrastructure.Policies;
using System;
using System.Collections.Generic;

namespace Modules.Blog.Infrastructure.Policies;

internal sealed class BlogPolicyFactory : IPolicyFactory
{
    public string ModuleName => "Blog";

    public Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies()
    {
        return new Dictionary<string, Action<AuthorizationPolicyBuilder>>
        {
            // Requires EITHER Admin role OR the specific manage:all claim
            [BlogPostPolicyConsts.ManageAllPostsPolicy] = policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") || // Check Admin role
                    context.User.HasClaim(c => c.Type == BlogPostPolicyConsts.ManageAllPostsPolicy && c.Value == "true")
                ),

            // Requires the user to simply be authenticated (logged in)
            [BlogPostPolicyConsts.AddCommentsPolicy] = policy =>
                policy.RequireAuthenticatedUser()
        };
        // Note: ManageOwnPostsPolicy often requires resource-based authorization logic
        // within the handler (checking if post.AuthorId == currentUser.Id),
        // so we don't define a simple policy for it here.
    }
}