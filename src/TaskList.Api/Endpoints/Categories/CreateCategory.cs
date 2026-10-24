using TaskList.Api.Contracts;
using TaskList.Api.Contracts.Categories;
using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Categories.Create;

namespace TaskList.Api.Endpoints.Categories;

internal sealed class CreateCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/categories", async (
                CreateCategoryRequest request,
                ICommandHandler<CreateCategoryCommand, Guid> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(
                    new CreateCategoryCommand(request.Name, request.Description, request.Color),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/categories/{result.Value}", new CreatedResponse(result.Value))
                    : result.ToProblem();
            })
            .WithName("CreateCategory")
            .WithTags(Tags.Categories)
            .WithSummary("Create a category")
            .Produces<CreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
}
