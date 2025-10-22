using Modules.Blog.Features.Posts.GetPostBySlug; // Use handler interface
using Modules.Blog.Features.Posts.Shared.Responses; // Use response DTO
using Modules.Common.Application.Caching; // Use ICachingService
using Modules.Common.Domain.Results; // Use Result<>
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Decorators;

// Decorator specific to IGetPostBySlugHandler
internal sealed class CachingGetPostBySlugHandlerDecorator(IGetPostBySlugHandler decorated, ICachingService cachingService) : IGetPostBySlugHandler
{
    private readonly IGetPostBySlugHandler _decorated = decorated; // The actual handler
    private readonly ICachingService _cachingService = cachingService; // Caching implementation

    public async Task<Result<PostResponse>> HandleAsync(string slug, CancellationToken cancellationToken)
    {
        // 1. Define Cache Key and Expiration
        string cacheKey = $"post:{slug}"; // Use slug in the key
        TimeSpan? expiration = TimeSpan.FromMinutes(5); // Cache posts for 5 minutes

        // 2. Try getting from cache
        var cachedResult = await _cachingService.GetAsync<PostResponse>(cacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            return cachedResult; // Cache HIT
        }

        // 3. Cache MISS: Execute the actual handler
        var result = await _decorated.HandleAsync(slug, cancellationToken);

        // 4. If successful, store in cache
        if (result.IsSuccess && result.Value is not null)
        {
            await _cachingService.SetAsync(cacheKey, result.Value, expiration, cancellationToken);
        }

        // 5. Return the result from the actual handler
        return result;
    }
}