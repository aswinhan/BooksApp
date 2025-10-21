using FluentValidation; // For IValidator
using Microsoft.AspNetCore.Builder; // For WebApplication
using Microsoft.AspNetCore.Http; // For IResult, Results
using Microsoft.AspNetCore.Mvc; // For FromBody
using Modules.Common.API.Abstractions; // For IApiEndpoint
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed for extension
using Modules.Users.Features.Users.Shared; // For UserResponse
using Modules.Users.Features.Users.Shared.Routes; // For RouteConsts
using System.Threading; // For CancellationToken
using System.Threading.Tasks; // For Task

namespace Modules.Users.Features.Users.RegisterUser;

public class RegisterUserEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Map POST request to the /register route, allow anonymous access
        app.MapPost(RouteConsts.Register, Handle)
           .AllowAnonymous() // No authentication required
           .WithName("RegisterUser") // Endpoint name for linking/logging
           .Produces<UserResponse>(StatusCodes.Status201Created) // Success response type
           .ProducesValidationProblem() // Standard validation error response
           .ProducesProblem(StatusCodes.Status409Conflict) // Conflict error response
           .WithTags("Users"); // Group in Swagger UI
    }

    private static async Task<IResult> Handle(
        [FromBody] RegisterUserRequest request, // Get data from request body
        IValidator<RegisterUserRequest> validator, // Inject validator
        IRegisterUserHandler handler, // Inject the handler
        CancellationToken cancellationToken)
    {
        // 1. Validate the request
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            // Return standard validation problem response
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // 2. Call the handler logic
        var response = await handler.HandleAsync(request, cancellationToken);

        // 3. Handle potential errors using our extension method
        if (response.IsError)
        {
            // ToProblem() converts domain errors (like Conflict) to HTTP results
            return response.Errors.ToProblem();
        }

        // 4. Return success response (HTTP 201 Created)
        // Creates a response with a Location header pointing to the new user's URL
        return Results.CreatedAtRoute("GetUserById", // Route name matches GetUserByIdEndpoint's WithName
                                      new { userId = response.Value?.Id }, // Route parameters
                                      response.Value); // Response body
    }
}