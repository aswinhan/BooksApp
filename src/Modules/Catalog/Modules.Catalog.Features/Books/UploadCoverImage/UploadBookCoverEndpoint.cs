using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http; // Required for IFormFile, Results
using Microsoft.AspNetCore.Mvc; // Required for FromRoute
using Modules.Catalog.Domain.Policies; // Use policies
using Modules.Catalog.Features.Books.Shared.Responses; // Use BookResponse
using Modules.Catalog.Features.Books.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;

namespace Modules.Catalog.Features.Books.UploadCoverImage;

public class UploadBookCoverEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Use POST or PUT for uploading/replacing the cover
        app.MapPost(BookRouteConsts.UploadCoverImage, Handle)
           .RequireAuthorization(CatalogPolicyConsts.ManageCatalogPolicy) // Secure: Only admins/managers
           .Accepts<IFormFile>("multipart/form-data") // Specify expected content type for file uploads
           .WithName("UploadBookCover")
           .Produces<BookResponse>(StatusCodes.Status200OK) // Return updated book
           .ProducesValidationProblem() // File validation (size, type)
           .ProducesProblem(StatusCodes.Status404NotFound) // Book not found
           .ProducesProblem(StatusCodes.Status400BadRequest) // File issues
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Catalog.Books");
    }

    // Handle method now expects an IFormFile
    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId,
        HttpRequest request, // Inject HttpRequest to access the form file
        IUploadBookCoverHandler handler,
        CancellationToken cancellationToken)
    {
        // --- File Validation ---
        if (!request.HasFormContentType)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                { "File", ["Request must be multipart/form-data."] }
            });
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file"); // Assuming the input field name is "file"

        if (file == null || file.Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                { "File", ["No file uploaded or file is empty."] }
            });
        }

        // Optional: Add more validation (file size, content type)
        const long maxFileSize = 5 * 1024 * 1024; // 5 MB example limit
        if (file.Length > maxFileSize)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                { "File", ["File size exceeds the limit (5MB)."] }
            });
        }
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                { "File", ["Invalid file type. Only JPG, PNG, WEBP are allowed."] }
            });
        }
        // --- End File Validation ---


        var response = await handler.HandleAsync(bookId, file, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Handles NotFound, etc.
        }

        return Results.Ok(response.Value); // Return updated BookResponse
    }
}