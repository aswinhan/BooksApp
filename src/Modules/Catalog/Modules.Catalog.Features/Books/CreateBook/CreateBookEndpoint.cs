using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Catalog.Domain.Policies;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

// using Modules.Common.API.Extensions; // Namespace changed
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.CreateBook;

public class CreateBookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(BookRouteConsts.CreateBook, Handle)
           .RequireAuthorization(CatalogPolicyConsts.ManageCatalogPolicy)
           .WithName("CreateBook")
           .Produces<BookResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status400BadRequest) // For other errors like Author not found
           .WithTags("Catalog.Books"); // Group in Swagger
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateBookRequest request,
        IValidator<CreateBookRequest> validator,
        ICreateBookHandler handler,
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
            // ToProblem handles NotFound (for Author), Validation etc.
            return response.Errors.ToProblem();
        }

        // Return 201 Created with location header and response body
        return Results.CreatedAtRoute("GetBookById", // Use GetBookById endpoint name
                                      new { bookId = response.Value?.Id }, // Route parameter
                                      response.Value); // Response body
    }
}