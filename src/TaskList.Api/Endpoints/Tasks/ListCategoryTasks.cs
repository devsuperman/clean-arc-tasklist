using TaskList.Api.Contracts.Tasks;
using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks;
using TaskList.Application.Tasks.ListByCategory;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class ListCategoryTasks : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/categories/{id:guid}/tasks", async (
                Guid id,
                [AsParameters] ListCategoryTasksRequest request,
                IQueryHandler<ListCategoryTasksQuery, PagedResult<TaskResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new ListCategoryTasksQuery(
                    id,
                    request.Status,
                    request.Priority,
                    request.DueFrom,
                    request.DueTo,
                    request.Overdue,
                    request.Search,
                    request.SortBy,
                    request.SortDirection,
                    request.Page,
                    request.PageSize);
                var result = await handler.Handle(query, cancellationToken);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .WithName("ListCategoryTasks")
            .WithTags(Tags.Tasks)
            .WithSummary("List the tasks of a category with filters, sorting and paging")
            .Produces<PagedResult<TaskResponse>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
}
