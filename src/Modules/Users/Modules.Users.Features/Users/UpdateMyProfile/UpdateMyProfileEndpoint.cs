using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Users.Features.Users.GetMyProfile; // Reuse GetMyProfileResponse
using Modules.Users.Features.Users.Shared.Routes; // Use base route
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Users.Features.Users.UpdateMyProfile;

public class UpdateMyProfileEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(RouteConsts.UpdateMyProfile, Handle)
           .RequireAuthorization() // Must be logged in
           .WithName("UpdateMyProfile")
           .Produces<GetMyProfileResponse>(StatusCodes.Status200OK) // Return updated profile
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Users.Profile"); // New tag
    }

    private static async Task<IResult> Handle(
        [FromBody] UpdateMyProfileRequest request,
        IValidator<UpdateMyProfileRequest> validator,
        IUpdateMyProfileHandler handler,
        ClaimsPrincipal user, // Need user ID
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized(); // Should not happen
        }

        var response = await handler.HandleAsync(userId, request, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Handles NotFound, etc.
        }

        return Results.Ok(response.Value); // Return updated profile
    }
}