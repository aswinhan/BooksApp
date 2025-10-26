using System;
namespace Modules.Catalog.Features.Categories.Shared.Responses;

public record CategoryResponse(Guid Id, string Name, string Slug, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);