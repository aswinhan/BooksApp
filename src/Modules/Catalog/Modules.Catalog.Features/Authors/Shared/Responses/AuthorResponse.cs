using System;

namespace Modules.Catalog.Features.Authors.Shared.Responses;

// DTO for returning author details
public record AuthorResponse(
    Guid Id,
    string Name,
    string? Biography,
    int BookCount,
    int TotalSales,
    int TotalReviews,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);