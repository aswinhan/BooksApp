namespace Modules.Catalog.PublicApi.Contracts;

// DTO for potentially sharing basic book details internally
public record BookDetailsDto(
    Guid Id,
    string Title,
    decimal Price,
    string AuthorName,
    string? CoverImageUrl
// Add other properties needed by other modules, e.g., Isbn
);