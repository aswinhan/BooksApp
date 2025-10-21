using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using Modules.Users.Domain.Authentication; // For LoginUserResponse
using Modules.Users.Features.Users.Shared.Routes;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Users.Features.Users.LoginUser;

public class LoginUserEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(RouteConsts.Login, Handle)
           .AllowAnonymous()
           .WithName("LoginUser")
           .Produces<LoginUserResponse>(StatusCodes.Status200OK) // Success
           .ProducesValidationProblem() // Validation failure
           .ProducesProblem(StatusCodes.Status401Unauthorized) // Auth failure
           .WithTags("Users");
    }

    private static async Task<IResult> Handle(
        [FromBody] LoginUserRequest request,
        IValidator<LoginUserRequest> validator,
        ILoginUserHandler handler, // Inject specific handler
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(request, cancellationToken);

        // Handle domain errors (like InvalidCredentials)
        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Converts Unauthorized error to 401
        }

        // Return success (HTTP 200 OK with token response)
        return Results.Ok(response.Value);
    }
}