using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Paging;
using TaskList.Domain.Common;

namespace TaskList.Application.Categories.List;

internal sealed class ListCategoriesQueryHandler(IReadDbContext db)
    : IQueryHandler<ListCategoriesQuery, PagedResult<CategoryResponse>>
{
    public async Task<Result<PagedResult<CategoryResponse>>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = db.Categories;

        // BR-L06: case-insensitive "contains" on Name; a blank search is ignored.
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
#pragma warning disable CA1304, CA1311, CA1862 // ToLower() is translated to SQL lower().
            var search = query.Search.Trim().ToLower();
            categories = categories.Where(c => c.Name.ToLower().Contains(search));
#pragma warning restore CA1304, CA1311, CA1862
        }

        // BR-C06: ordered by name.
        var responses = categories.OrderBy(c => c.Name).SelectResponse(db.Tasks);

        return await db.ToPagedResultAsync(responses, query, cancellationToken);
    }
}
