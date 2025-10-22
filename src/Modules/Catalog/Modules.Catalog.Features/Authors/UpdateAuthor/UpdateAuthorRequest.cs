namespace Modules.Catalog.Features.Authors.UpdateAuthor;

// Use same DTO as Create for simplicity, or create a specific one
public record UpdateAuthorRequest(string Name, string? Biography);