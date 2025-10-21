using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Authentication; // Needs IClientAuthService & Response DTO

namespace Modules.Users.Features.Users.RefreshToken;

// Interface for the handler
internal interface IRefreshTokenHandler : IHandler
{
    Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}

// Implementation delegates to the Infrastructure service
internal sealed class RefreshTokenHandler(
    IClientAuthorizationService authorizationService, // Inject the service
    ILogger<RefreshTokenHandler> logger)
    : IRefreshTokenHandler
{
    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling token refresh request.");
        // Call the authorization service which contains the refresh logic
        var result = await authorizationService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Token refresh handler failed: {Error}", result.FirstError.Code);
        }
        else
        {
            logger.LogInformation("Token refresh handler succeeded.");
        }
        return result;
    }
}