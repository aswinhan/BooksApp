using FluentValidation;
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

namespace Modules.Users.Features.Users.UpdateUserRole;

public class UpdateUserRoleEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Map POST request to /api/users/{userId}/role
        // Using POST instead of PUT/PATCH as it's a specific action
        app.MapPost(RouteConsts.UpdateUserRole, Handle)
           // Secure endpoint using the "update" policy (often role changes require high privilege)
           .RequireAuthorization(UserPolicyConsts.UpdatePolicy)
           .WithName("UpdateUserRole")
           .Produces(StatusCodes.Status204NoContent) // Success response (no body)
           .ProducesValidationProblem() // Validation errors
           .ProducesProblem(StatusCodes.Status404NotFound) // User or Role not found
           .ProducesProblem(StatusCodes.Status400BadRequest) // General failure
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Users");
    }

    private static async Task<IResult> Handle(
        [FromRoute] string userId, // Get ID from route
        [FromBody] UpdateUserRoleRequest request, // Get data from body
        IValidator<UpdateUserRoleRequest> validator, // Inject validator
        IUpdateUserRoleHandler handler, // Inject specific handler
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(userId, request, cancellationToken);

        if (response.IsError)
        {
            // ToProblem handles NotFound, Failure errors
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 204 No Content)
        return Results.NoContent();
    }
}