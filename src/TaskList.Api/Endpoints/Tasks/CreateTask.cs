using TaskList.Api.Contracts;
using TaskList.Api.Contracts.Tasks;
using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Tasks.Create;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class CreateTask : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/tasks", async (
                CreateTaskRequest request,
                ICommandHandler<CreateTaskCommand, Guid> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateTaskCommand(
                    request.Title,
                    request.Description,
                    request.CategoryId,
                    request.Priority,
                    request.DueDate);

                var result = await handler.Handle(command, cancellationToken);
                return result.IsSuccess
                    ? Results.Created($"/api/v1/tasks/{result.Value}", new CreatedResponse(result.Value))
                    : result.ToProblem();
            })
            .WithName("CreateTask")
            .WithTags(Tags.Tasks)
            .WithSummary("Create a task")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
