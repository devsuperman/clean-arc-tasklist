using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common;
using TaskList.Domain.Common;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.GetById;

internal sealed class GetTaskByIdQueryHandler(IReadDbContext db, TimeProvider timeProvider)
    : IQueryHandler<GetTaskByIdQuery, TaskResponse>
{
    public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var task = await db.FirstOrDefaultAsync(
            db.Tasks.Where(task => task.Id == query.Id).SelectResponse(db.Categories, timeProvider.Today()),
            cancellationToken);

        return task is null ? TaskErrors.NotFound(query.Id) : task;
    }
}
