using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using Modules.Users.Domain.Authentication; // For RefreshTokenResponse
using Modules.Users.Features.Users.Shared.Routes;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Users.Features.Users.RefreshToken;

public class RefreshTokenEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(RouteConsts.RefreshToken, Handle)
           .AllowAnonymous() // Refreshing doesn't require an active (non-expired) JWT
           .WithName("RefreshToken")
           .Produces<RefreshTokenResponse>(StatusCodes.Status200OK) // Success
           .ProducesValidationProblem() // Validation failure
           .ProducesProblem(StatusCodes.Status401Unauthorized) // Token invalid/expired
           .WithTags("Users");
    }

    private static async Task<IResult> Handle(
        [FromBody] RefreshTokenRequest request,
        IValidator<RefreshTokenRequest> validator,
        IRefreshTokenHandler handler, // Inject specific handler
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(request, cancellationToken);

        if (response.IsError)
        {
            // ToProblem converts Unauthorized error to 401
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 200 OK with new tokens)
        return Results.Ok(response.Value);
    }
}