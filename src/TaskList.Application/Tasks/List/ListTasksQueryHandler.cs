using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common;
using TaskList.Application.Common.Paging;
using TaskList.Domain.Common;

namespace TaskList.Application.Tasks.List;

internal sealed class ListTasksQueryHandler(IReadDbContext db, TimeProvider timeProvider)
    : IQueryHandler<ListTasksQuery, PagedResult<TaskResponse>>
{
    public async Task<Result<PagedResult<TaskResponse>>> Handle(ListTasksQuery query, CancellationToken cancellationToken) =>
        await db.ListTasksAsync(query.CategoryId, query, timeProvider.Today(), cancellationToken);
}
