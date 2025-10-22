using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Features.Authors.Shared.Responses;
using Modules.Catalog.Features.Authors.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Authors.CreateAuthor;

public class CreateAuthorEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(AuthorRouteConsts.CreateAuthor, Handle)
           .RequireAuthorization() // Add admin policy later
           .WithName("CreateAuthor")
           .Produces<AuthorResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Catalog.Authors");
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateAuthorRequest request,
        IValidator<CreateAuthorRequest> validator,
        ICreateAuthorHandler handler,
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
            // ToProblem handles potential errors (e.g., Conflict if name must be unique)
            return response.Errors.ToProblem();
        }

        // Return 201 Created (consider adding GetAuthorById later for Location header)
        return Results.Created(AuthorRouteConsts.BaseRoute + $"/{response.Value?.Id}", response.Value);
    }
}