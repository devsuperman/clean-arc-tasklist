using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Paging;

namespace TaskList.Application.Categories.List;

/// <summary>UC-03: a page of categories ordered by name (BR-C06), optionally filtered by <paramref name="Search"/> (BR-L06).</summary>
public sealed record ListCategoriesQuery(
    int Page = Paging.DefaultPage,
    int PageSize = Paging.DefaultPageSize,
    string? Search = null) : IQuery<PagedResult<CategoryResponse>>, IPagedQuery;
