namespace Modules.Catalog.Features.Books.Shared.Responses;

public record ReviewResponse(
    Guid Id,
    string UserId, // Keep as string to match User ID type
    string Comment,
    int RatingValue, // Expose the primitive value
    DateTime CreatedAtUtc
);