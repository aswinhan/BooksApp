using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Authentication; // Needs IClientAuthorizationService

namespace Modules.Users.Features.Users.UpdateUserRole;

// Interface for the handler
internal interface IUpdateUserRoleHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string userId, UpdateUserRoleRequest request, CancellationToken cancellationToken);
}

// Implementation delegates to the Infrastructure service
internal sealed class UpdateUserRoleHandler(
    IClientAuthorizationService authorizationService, // Inject the service
    ILogger<UpdateUserRoleHandler> logger)
    : IUpdateUserRoleHandler
{
    public async Task<Result<Success>> HandleAsync(
        string userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling update role request for user {UserId} to {NewRole}", userId, request.NewRole);
        // Call the authorization service which contains the logic including token invalidation
        var result = await authorizationService.UpdateUserRoleAsync(userId, request.NewRole, cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Update role handler failed for user {UserId}: {Error}", userId, result.FirstError.Code);
        }
        else
        {
            logger.LogInformation("Update role handler succeeded for user {UserId} to {NewRole}", userId, request.NewRole);
        }

        return result;
    }
}