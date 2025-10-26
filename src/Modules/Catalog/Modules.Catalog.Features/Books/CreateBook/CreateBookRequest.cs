using System;

namespace Modules.Catalog.Features.Books.CreateBook;

public record CreateBookRequest(
    string Title,
    string? Description,
    string Isbn,
    decimal Price,
    string? CoverImageUrl,
    Guid AuthorId, // We need the Author's ID
    Guid CategoryId
);