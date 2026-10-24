using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Tasks.ChangeStatus;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class StartTask : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/tasks/{id:guid}/start", async (
                Guid id,
                ICommandHandler<StartTaskCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new StartTaskCommand(id), cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            })
            .WithName("StartTask")
            .WithTags(Tags.Tasks)
            .WithSummary("Start a task")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
