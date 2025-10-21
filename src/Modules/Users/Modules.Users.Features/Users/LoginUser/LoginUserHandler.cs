using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Added logger
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Users.Domain.Authentication; // Needs IClientAuthService & Response DTO

namespace Modules.Users.Features.Users.LoginUser;

// Interface for the handler
internal interface ILoginUserHandler : IHandler
{
    Task<Result<LoginUserResponse>> HandleAsync(LoginUserRequest request, CancellationToken cancellationToken);
}

// Implementation delegates actual logic to the Infrastructure service
internal sealed class LoginUserHandler(
    IClientAuthorizationService authorizationService, // Inject the service
    ILogger<LoginUserHandler> logger) // Added logger
    : ILoginUserHandler
{
    public async Task<Result<LoginUserResponse>> HandleAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling login request for {Email}", request.Email);
        // Simply call the authorization service which contains the complex logic
        var result = await authorizationService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (result.IsError)
        {
            logger.LogWarning("Login handler failed for {Email}: {Error}", request.Email, result.FirstError.Code);
        }
        else
        {
            logger.LogInformation("Login handler succeeded for {Email}", request.Email);
        }

        return result;
    }
}