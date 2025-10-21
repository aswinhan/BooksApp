using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using Modules.Users.Domain.Policies; // For UserPolicyConsts
using Modules.Users.Features.Users.Shared; // For UserResponse
using Modules.Users.Features.Users.Shared.Routes; // For RouteConsts
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Users.Features.Users.UpdateUser;

public class UpdateUserEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Map PUT request to /api/users/{userId}
        app.MapPut(RouteConsts.UpdateUser, Handle)
           // Secure endpoint using the "update" policy
           .RequireAuthorization(UserPolicyConsts.UpdatePolicy)
           .WithName("UpdateUser")
           .Produces<UserResponse>(StatusCodes.Status200OK) // Success response
           .ProducesValidationProblem() // Validation errors
           .ProducesProblem(StatusCodes.Status404NotFound) // User not found
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Users");
    }

    private static async Task<IResult> Handle(
        [FromRoute] string userId, // Get ID from the route
        [FromBody] UpdateUserRequest request, // Get data from the body
        IValidator<UpdateUserRequest> validator, // Inject validator
        IUpdateUserHandler handler, // Inject specific handler
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
            // ToProblem handles NotFound, Validation, Failure errors
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 200 OK with updated user data)
        return Results.Ok(response.Value);
    }
}