using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Tasks.Delete;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class DeleteTask : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete("/tasks/{id:guid}", async (
                Guid id,
                ICommandHandler<DeleteTaskCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new DeleteTaskCommand(id), cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            })
            .WithName("DeleteTask")
            .WithTags(Tags.Tasks)
            .WithSummary("Delete a task")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
