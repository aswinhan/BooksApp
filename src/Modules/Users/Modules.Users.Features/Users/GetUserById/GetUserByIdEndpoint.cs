using Microsoft.AspNetCore.Builder; // For WebApplication, RouteHandlerBuilder
using Microsoft.AspNetCore.Http; // For IResult, Results
using Microsoft.AspNetCore.Mvc; // For FromRoute
using Modules.Common.API.Abstractions; // For IApiEndpoint
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using Modules.Users.Domain.Policies; // For UserPolicyConsts
using Modules.Users.Features.Users.Shared; // For UserResponse
using Modules.Users.Features.Users.Shared.Routes; // For RouteConsts
using System.Threading; // For CancellationToken
using System.Threading.Tasks; // For Task

namespace Modules.Users.Features.Users.GetUserById;

public class GetUserByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(RouteConsts.GetById, Handle)
           // Secure this endpoint using the "read" policy
           .RequireAuthorization(UserPolicyConsts.ReadPolicy)
           .WithName("GetUserById") // Name for linking
           .Produces<UserResponse>(StatusCodes.Status200OK) // Success response
           .ProducesProblem(StatusCodes.Status404NotFound) // Not found response
           .ProducesProblem(StatusCodes.Status401Unauthorized) // Not logged in
           .ProducesProblem(StatusCodes.Status403Forbidden) // Logged in but wrong permissions
           .WithTags("Users");
    }

    // Handler is simpler for GET requests, takes ID from route
    private static async Task<IResult> Handle(
        [FromRoute] string userId, // Get ID from the URL path
        IGetUserByIdHandler handler, // Inject the specific handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(userId, cancellationToken);

        if (response.IsError)
        {
            // ToProblem converts NotFound error to 404
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 200 OK with user data)
        return Results.Ok(response.Value);
    }
}