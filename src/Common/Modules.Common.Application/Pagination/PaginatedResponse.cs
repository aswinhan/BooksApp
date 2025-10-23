using System;
using System.Collections.Generic;

namespace Modules.Common.Application.Pagination;

public class PaginatedResponse<T>(List<T> items, int totalCount, int pageNumber, int pageSize)
{
    public List<T> Items { get; } = items;
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = totalCount;
    public int TotalPages { get; } = (int)Math.Ceiling(totalCount / (double)pageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}