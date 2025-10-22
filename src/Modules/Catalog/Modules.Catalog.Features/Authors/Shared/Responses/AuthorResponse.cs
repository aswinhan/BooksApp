using System;

namespace Modules.Catalog.Features.Authors.Shared.Responses;

// DTO for returning author details
public record AuthorResponse(
    Guid Id,
    string Name,
    string? Biography,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);