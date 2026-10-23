using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Tasks.ChangeStatus;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class ReopenTask : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/tasks/{id:guid}/reopen", async (
                Guid id,
                ICommandHandler<ReopenTaskCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new ReopenTaskCommand(id), cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            })
            .WithName("ReopenTask")
            .WithTags(Tags.Tasks)
            .WithSummary("Reopen a task")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
