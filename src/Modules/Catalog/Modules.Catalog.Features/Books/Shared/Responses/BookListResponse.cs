using System;

namespace Modules.Catalog.Features.Books.Shared.Responses;

// Simplified DTO for book lists
public record BookListResponse(
    Guid Id,
    string Title,
    string AuthorName, // Denormalized for convenience
    decimal Price,
    string? CoverImageUrl // Add later if needed
);