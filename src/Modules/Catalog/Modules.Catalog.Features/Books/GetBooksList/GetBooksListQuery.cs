using Modules.Catalog.Features.Books.Shared.Responses; // Use BookListResponse (to be created)
using Modules.Common.Domain.Results; // Use Result<>

namespace Modules.Catalog.Features.Books.GetBooksList;

// We might add pagination parameters later (e.g., page number, page size)
// For now, a simple query record.
public record GetBooksListQuery();