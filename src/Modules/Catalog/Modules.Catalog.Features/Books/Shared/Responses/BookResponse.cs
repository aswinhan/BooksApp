using System;
using System.Collections.Generic;

namespace Modules.Catalog.Features.Books.Shared.Responses;

// DTO for returning detailed book information
public record BookResponse(
    Guid Id,
    string Title,
    string? Description,
    string Isbn,
    decimal Price,
    Guid AuthorId,
    string AuthorName, // Include author name for convenience
    List<ReviewResponse> Reviews, // Include reviews
    int QuantityAvailable,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);