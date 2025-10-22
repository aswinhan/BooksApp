using Modules.Catalog.Features.Books.GetBookById; // Use handler interface
using Modules.Catalog.Features.Books.Shared.Responses; // Use response DTO
using Modules.Common.Application.Caching; // Use ICachingService
using Modules.Common.Domain.Results; // Use Result<>
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Decorators;

// Decorator specific to IGetBookByIdHandler
internal sealed class CachingGetBookByIdHandlerDecorator(IGetBookByIdHandler decorated, ICachingService cachingService) : IGetBookByIdHandler
{
    private readonly IGetBookByIdHandler _decorated = decorated; // The actual handler
    private readonly ICachingService _cachingService = cachingService; // Caching implementation

    public async Task<Result<BookResponse>> HandleAsync(Guid bookId, CancellationToken cancellationToken)
    {
        // 1. Define Cache Key and Expiration (Could come from an attribute later)
        string cacheKey = $"book:{bookId}";
        TimeSpan? expiration = TimeSpan.FromMinutes(10); // Cache for 10 minutes

        // 2. Try getting from cache
        var cachedResult = await _cachingService.GetAsync<BookResponse>(cacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            return cachedResult; // Cache HIT, return cached value (implicit conversion)
        }

        // 3. Cache MISS: Execute the actual handler
        var result = await _decorated.HandleAsync(bookId, cancellationToken);

        // 4. If successful, store in cache before returning
        if (result.IsSuccess && result.Value is not null)
        {
            await _cachingService.SetAsync(cacheKey, result.Value, expiration, cancellationToken);
        }

        // 5. Return the result from the actual handler
        return result;
    }
}