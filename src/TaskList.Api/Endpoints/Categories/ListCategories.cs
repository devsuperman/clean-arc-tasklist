using TaskList.Api.Contracts.Categories;
using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Categories;
using TaskList.Application.Categories.List;
using TaskList.Application.Common.Paging;

namespace TaskList.Api.Endpoints.Categories;

internal sealed class ListCategories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/categories", async (
                [AsParameters] ListCategoriesRequest request,
                IQueryHandler<ListCategoriesQuery, PagedResult<CategoryResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(
                    new ListCategoriesQuery(request.Page, request.PageSize, request.Search),
                    cancellationToken);

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .WithName("ListCategories")
            .WithTags(Tags.Categories)
            .WithSummary("List categories")
            .WithDescription("Paginated, ordered by name. `search` filters by name, ignoring case.")
            .Produces<PagedResult<CategoryResponse>>()
            .ProducesValidationProblem();
}
