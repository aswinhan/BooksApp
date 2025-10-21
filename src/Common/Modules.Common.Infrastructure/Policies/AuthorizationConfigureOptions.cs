using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic; // For IEnumerable

namespace Modules.Common.Infrastructure.Policies;

/// <summary>
/// Configures the application's AuthorizationOptions by discovering and adding
/// policies defined by all registered IPolicyFactory services.
/// </summary>
public class AuthorizationConfigureOptions(
    IEnumerable<IPolicyFactory> policyFactories, // Injects all IPolicyFactory implementations
    ILogger<AuthorizationConfigureOptions> logger)
    : IConfigureOptions<AuthorizationOptions> // Implements interface to configure options
{
    public void Configure(AuthorizationOptions options)
    {
        logger.LogInformation("Configuring authorization policies...");

        // Iterate through each module's policy factory
        foreach (var factory in policyFactories)
        {
            logger.LogInformation("Loading policies for module: {ModuleName}", factory.ModuleName);

            var policies = factory.GetPolicies();

            // Add each policy defined by the factory to the global options
            foreach (var (policyName, policyBuilderAction) in policies)
            {
                options.AddPolicy(policyName, policyBuilderAction);
                logger.LogDebug("Added policy: {PolicyName} from module {ModuleName}", policyName, factory.ModuleName);
            }
        }
        logger.LogInformation("Finished configuring authorization policies.");
    }
}