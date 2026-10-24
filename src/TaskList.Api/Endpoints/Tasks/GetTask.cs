using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Tasks;
using TaskList.Application.Tasks.GetById;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class GetTask : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/tasks/{id:guid}", async (
                Guid id,
                IQueryHandler<GetTaskByIdQuery, TaskResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new GetTaskByIdQuery(id), cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .WithName("GetTask")
            .WithTags(Tags.Tasks)
            .WithSummary("Get a task by id")
            .Produces<TaskResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
