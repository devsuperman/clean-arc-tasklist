using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Categories;
using TaskList.Application.Categories.Get;

namespace TaskList.Api.Endpoints.Categories;

internal sealed class GetCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/categories/{id:guid}", async (
                Guid id,
                IQueryHandler<GetCategoryQuery, CategoryResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new GetCategoryQuery(id), cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .WithName("GetCategory")
            .WithTags(Tags.Categories)
            .WithSummary("Get a category by id")
            .Produces<CategoryResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
