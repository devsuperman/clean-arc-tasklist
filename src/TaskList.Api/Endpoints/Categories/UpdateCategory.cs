using TaskList.Api.Contracts.Categories;
using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Categories.Update;

namespace TaskList.Api.Endpoints.Categories;

internal sealed class UpdateCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("/categories/{id:guid}", async (
                Guid id,
                UpdateCategoryRequest request,
                ICommandHandler<UpdateCategoryCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(
                    new UpdateCategoryCommand(id, request.Name, request.Description, request.Color),
                    cancellationToken);

                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            })
            .WithName("UpdateCategory")
            .WithTags(Tags.Categories)
            .WithSummary("Update a category")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
