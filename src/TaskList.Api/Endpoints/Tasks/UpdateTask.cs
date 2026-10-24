using TaskList.Api.Contracts.Tasks;
using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Tasks.Update;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class UpdateTask : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("/tasks/{id:guid}", async (
                Guid id,
                UpdateTaskRequest request,
                ICommandHandler<UpdateTaskCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateTaskCommand(
                    id,
                    request.Title,
                    request.Description,
                    request.CategoryId,
                    request.Priority,
                    request.DueDate);

                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            })
            .WithName("UpdateTask")
            .WithTags(Tags.Tasks)
            .WithSummary("Update a task")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
