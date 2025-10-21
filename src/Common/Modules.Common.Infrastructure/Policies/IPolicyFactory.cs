using Microsoft.AspNetCore.Authorization; // For AuthorizationPolicyBuilder
using System; // For Action<>
using System.Collections.Generic; // For Dictionary

namespace Modules.Common.Infrastructure.Policies;

/// <summary>
/// Interface for factories that define authorization policies specific to a module.
/// </summary>
public interface IPolicyFactory
{
    /// <summary>
    /// Gets the name of the module these policies belong to.
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Gets a dictionary mapping policy names to configuration actions.
    /// </summary>
    /// <returns>A dictionary where keys are policy names (e.g., "users:read")
    /// and values are actions to configure the AuthorizationPolicyBuilder.</returns>
    Dictionary<string, Action<AuthorizationPolicyBuilder>> GetPolicies();
}