using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Tasks.ChangeStatus;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class CompleteTask : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/tasks/{id:guid}/complete", async (
                Guid id,
                ICommandHandler<CompleteTaskCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new CompleteTaskCommand(id), cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            })
            .WithName("CompleteTask")
            .WithTags(Tags.Tasks)
            .WithSummary("Complete a task")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
