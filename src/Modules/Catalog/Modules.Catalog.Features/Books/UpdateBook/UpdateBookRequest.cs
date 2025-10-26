using System;

namespace Modules.Catalog.Features.Books.UpdateBook;

// DTO for the update request body
public record UpdateBookRequest(
    string Title,
    string? Description,
    string Isbn,
    decimal Price,
    string? CoverImageUrl,
    Guid AuthorId, // Allow changing the author
    Guid CategoryId
);