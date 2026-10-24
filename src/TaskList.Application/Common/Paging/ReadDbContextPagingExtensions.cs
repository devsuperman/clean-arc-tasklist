using TaskList.Application.Abstractions.Data;

namespace TaskList.Application.Common.Paging;

public static class ReadDbContextPagingExtensions
{
    /// <summary>Counts <paramref name="query"/> and returns the requested page of it (BR-L01, BR-L02). The query must already be sorted.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IReadDbContext db,
        IQueryable<T> query,
        IPagedQuery paging,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(paging);

        var totalCount = await db.CountAsync(query, cancellationToken);
        var items = await db.ToListAsync(
            query.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize),
            cancellationToken);

        return new PagedResult<T>(items, paging.Page, paging.PageSize, totalCount);
    }
}
