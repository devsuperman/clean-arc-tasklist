using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks.List;
using TaskList.Domain.Categories;
using TaskList.Domain.Common;

namespace TaskList.Application.Tasks.ListByCategory;

internal sealed class ListCategoryTasksQueryHandler(IReadDbContext db, TimeProvider timeProvider)
    : IQueryHandler<ListCategoryTasksQuery, PagedResult<TaskResponse>>
{
    public async Task<Result<PagedResult<TaskResponse>>> Handle(
        ListCategoryTasksQuery query,
        CancellationToken cancellationToken)
    {
        var categoryExists = await db.CountAsync(
            db.Categories.Where(category => category.Id == query.CategoryId),
            cancellationToken) > 0;
        if (!categoryExists)
        {
            return CategoryErrors.NotFound(query.CategoryId);
        }

        return await db.ListTasksAsync(query.CategoryId, query, timeProvider.Today(), cancellationToken);
    }
}
