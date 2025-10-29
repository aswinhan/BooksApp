using System;

namespace Modules.Wishlist.Features.Features.Shared.Responses;

// DTO for returning wishlist items
// Includes book details fetched from Catalog module
public record WishlistItemResponse(
    Guid BookId,
    string Title,
    string AuthorName,
    decimal Price,
    string? CoverImageUrl,
    int QuantityAvailable, // From Inventory
    DateTime AddedAtUtc
);