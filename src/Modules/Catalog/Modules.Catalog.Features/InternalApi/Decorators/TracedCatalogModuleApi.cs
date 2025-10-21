namespace Modules.Catalog.Features.InternalApi.Decorators
{
    using Modules.Catalog.Features.Tracing;
    using Modules.Catalog.PublicApi;
    using Modules.Catalog.PublicApi.Contracts;
    using Modules.Common.Domain.Results;
    using System.Diagnostics;

    internal class TracedCatalogModuleApi(ICatalogModuleApi inner) : ICatalogModuleApi
    {
        public async Task<Result<BookDetailsDto>> GetBookByIdAsync(Guid bookId, CancellationToken cancellationToken)
        {
            // Start a new activity span for this internal API call
            using var activity = CatalogActivitySource.Instance.StartActivity($"{CatalogActivitySource.Instance.Name}.get-book-by-id");

            activity?.SetTag("module", CatalogActivitySource.Instance.Name);
            activity?.SetTag("operation", "GetBookById");
            activity?.SetTag("book.id", bookId.ToString());

            try
            {
                var response = await inner.GetBookByIdAsync(bookId, cancellationToken);

                activity?.SetTag("book.found", response.IsSuccess);
                activity?.SetStatus(response.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error); // Set status based on result

                return response;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error", true);
                activity?.SetTag("error.message", ex.Message);
                throw; // Rethrow exception
            }
        }
    }
}