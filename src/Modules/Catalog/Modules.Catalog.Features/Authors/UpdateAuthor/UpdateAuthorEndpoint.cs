using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Domain.Policies;
using Modules.Catalog.Features.Authors.Shared.Responses;
using Modules.Catalog.Features.Authors.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Authors.UpdateAuthor;

public class UpdateAuthorEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(AuthorRouteConsts.UpdateAuthor, Handle)
           .RequireAuthorization(CatalogPolicyConsts.ManageCatalogPolicy)// Add admin policy later
           .WithName("UpdateAuthor")
           .Produces<AuthorResponse>(StatusCodes.Status200OK)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Catalog.Authors");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid authorId,
        [FromBody] UpdateAuthorRequest request,
        IValidator<UpdateAuthorRequest> validator,
        IUpdateAuthorHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(authorId, request, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Handles NotFound, etc.
        }

        return Results.Ok(response.Value); // Return 200 OK with updated author
    }
}