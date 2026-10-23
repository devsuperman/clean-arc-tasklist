using TaskList.Api.Contracts.Tasks;
using TaskList.Api.Infrastructure;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks;
using TaskList.Application.Tasks.List;

namespace TaskList.Api.Endpoints.Tasks;

internal sealed class ListTasks : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/tasks", async (
                [AsParameters] ListTasksRequest request,
                IQueryHandler<ListTasksQuery, PagedResult<TaskResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new ListTasksQuery(
                    request.CategoryId,
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
            .WithName("ListTasks")
            .WithTags(Tags.Tasks)
            .WithSummary("List tasks with filters, sorting and paging")
            .Produces<PagedResult<TaskResponse>>()
            .ProducesValidationProblem();
}
