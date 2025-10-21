namespace Modules.Catalog.Features.Books.AddBookReview;

// DTO for the review request body
public record AddBookReviewRequest(
    string Comment,
    int Rating // Simple integer for the request
);