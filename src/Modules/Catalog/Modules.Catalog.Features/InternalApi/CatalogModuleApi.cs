using System;
using System.Threading;
using System.Threading.Tasks;
using Modules.Catalog.Features.Books.GetBookById; // Need the handler interface
using Modules.Catalog.PublicApi; // Need ICatalogModuleApi
using Modules.Catalog.PublicApi.Contracts; // Need DTOs
using Modules.Common.Domain.Results; // Need Result<>

namespace Modules.Catalog.Features.InternalApi;

// Internal implementation of the public API contract
internal sealed class CatalogModuleApi(
    IGetBookByIdHandler getBookByIdHandler // Inject the specific handler needed
                                           // Inject other handlers here as needed for other interface methods
    ) : ICatalogModuleApi
{
    public async Task<Result<BookDetailsDto>> GetBookByIdAsync(Guid bookId, CancellationToken cancellationToken)
    {
        // 1. Call the internal GetBookByIdHandler
        var result = await getBookByIdHandler.HandleAsync(bookId, cancellationToken);

        // 2. If the handler failed (e.g., book not found), return its error
        if (result.IsError)
        {
            return result.Errors!; // Pass the errors through
        }

        // 3. Map the full BookResponse from the handler to the limited BookDetailsDto
        var bookResponse = result.Value;
        var bookDetailsDto = new BookDetailsDto(
            bookResponse!.Id,
            bookResponse.Title,
            bookResponse.Price,
            bookResponse.AuthorName,    // <-- PASS THE VALUE
            bookResponse.CoverImageUrl
        );

        // 4. Return the successful DTO
        return bookDetailsDto; // Implicit conversion to Result<BookDetailsDto>
    }

    // Implement other ICatalogModuleApi methods here...
}