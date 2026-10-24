using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Categories.Delete;

namespace TaskList.Api.Endpoints.Categories;

internal sealed class DeleteCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapDelete("/categories/{id:guid}", async (
                Guid id,
                ICommandHandler<DeleteCategoryCommand> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new DeleteCategoryCommand(id), cancellationToken);
                return result.IsSuccess ? Results.NoContent() : result.ToProblem();
            })
            .WithName("DeleteCategory")
            .WithTags(Tags.Categories)
            .WithSummary("Delete a category")
            .WithDescription("Fails with 409 `Category.HasTasks` while the category has tasks.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
}
