using System.Net;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks;
using TaskList.Domain.Tasks;

namespace TaskList.Api.IntegrationTests.Tasks;

public sealed class ListCategoryTasksTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // UC-14, BR-L02
    public async Task List_ExistingCategory_Returns200WithOnlyItsTasks()
    {
        var work = await SeedCategoryAsync("Work");
        var home = await SeedCategoryAsync("Home");
        await SeedTaskAsync(work.Id, title: "Work old", createdAt: Clock.GetUtcNow().AddDays(-1));
        await SeedTaskAsync(work.Id, title: "Work new");
        await SeedTaskAsync(home.Id, title: "Home task");

        var response = await Client.GetAsync(new Uri($"/api/v1/categories/{work.Id}/tasks", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await ReadAsync<PagedResult<TaskResponse>>(response);
        page.Items.Select(item => item.Title).ShouldBe(["Work new", "Work old"]); // BR-L05 default sort
        page.Items.ShouldAllBe(item => item.Category.Id == work.Id);
        page.TotalCount.ShouldBe(2);
    }

    [Fact] // UC-14, BR-L03, BR-L05
    public async Task List_WithFiltersAndSort_AppliesTheSameRulesAsTaskList()
    {
        var work = await SeedCategoryAsync("Work");
        var home = await SeedCategoryAsync("Home");
        await SeedTaskAsync(work.Id, title: "Report low", priority: TaskPriority.Low);
        await SeedTaskAsync(work.Id, title: "Report high", priority: TaskPriority.High);
        await SeedTaskAsync(work.Id, title: "Report done", status: TaskStatus.Done);
        await SeedTaskAsync(work.Id, title: "Other");
        await SeedTaskAsync(home.Id, title: "Home report");

        var response = await Client.GetAsync(
            new Uri($"/api/v1/categories/{work.Id}/tasks?status=Pending&search=report&sortBy=priority&sortDirection=desc", UriKind.Relative),
            CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await ReadAsync<PagedResult<TaskResponse>>(response);
        page.Items.Select(item => item.Title).ShouldBe(["Report high", "Report low"]);
    }

    [Fact] // BR-L01, BR-L04
    public async Task List_InvalidParameters_Returns400()
    {
        var work = await SeedCategoryAsync();

        var response = await Client.GetAsync(
            new Uri($"/api/v1/categories/{work.Id}/tasks?dueFrom=2026-11-02&dueTo=2026-11-01&pageSize=101", UriKind.Relative),
            CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Code.ShouldBe("Validation.Failed");
        problem.Errors.ShouldNotBeNull().Keys.ShouldBe(["dueTo", "pageSize"], ignoreOrder: true);
    }

    [Fact]
    public async Task List_UnknownCategory_Returns404()
    {
        var response = await Client.GetAsync(new Uri($"/api/v1/categories/{Guid.NewGuid()}/tasks", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Category.NotFound");
    }
}
