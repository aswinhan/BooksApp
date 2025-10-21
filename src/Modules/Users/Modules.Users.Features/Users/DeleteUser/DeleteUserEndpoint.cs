using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using Modules.Users.Domain.Policies;
using Modules.Users.Features.Users.Shared.Routes;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Users.Features.Users.DeleteUser;

public class DeleteUserEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Map DELETE request to /api/users/{userId}
        app.MapDelete(RouteConsts.DeleteUser, Handle)
           // Secure endpoint using the "delete" policy
           .RequireAuthorization(UserPolicyConsts.DeletePolicy)
           .WithName("DeleteUser")
           .Produces(StatusCodes.Status204NoContent) // Success response (no body)
           .ProducesProblem(StatusCodes.Status404NotFound) // User not found
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Users");
    }

    private static async Task<IResult> Handle(
        [FromRoute] string userId, // Get ID from the route
        IDeleteUserHandler handler, // Inject specific handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(userId, cancellationToken);

        if (response.IsError)
        {
            // ToProblem handles NotFound, Failure errors
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 204 No Content)
        return Results.NoContent();
    }
}