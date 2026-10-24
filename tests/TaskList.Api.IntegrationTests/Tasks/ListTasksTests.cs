using System.Net;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks;
using TaskList.Domain.Tasks;

namespace TaskList.Api.IntegrationTests.Tasks;

public sealed class ListTasksTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // BR-L01, BR-L02, BR-L05
    public async Task List_WithoutParameters_ReturnsFirstPageSortedByCreatedAtDescending()
    {
        var work = await SeedCategoryAsync("Work", color: "#1E90FF");
        await SeedTaskAsync(work.Id, title: "Old", createdAt: Clock.GetUtcNow().AddDays(-2));
        await SeedTaskAsync(work.Id, title: "New");
        await SeedTaskAsync(work.Id, title: "Mid", createdAt: Clock.GetUtcNow().AddDays(-1));

        var response = await Client.GetAsync(new Uri("/api/v1/tasks", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await ReadAsync<PagedResult<TaskResponse>>(response);
        page.Items.Select(item => item.Title).ShouldBe(["New", "Mid", "Old"]);
        page.Page.ShouldBe(1);
        page.PageSize.ShouldBe(20);
        page.TotalCount.ShouldBe(3);
        page.TotalPages.ShouldBe(1);
        page.Items[0].Category.ShouldBe(new CategorySummary(work.Id, "Work", "#1E90FF"));
    }

    [Fact] // BR-L03
    public async Task List_FilterByCategory_ReturnsOnlyThatCategory()
    {
        var work = await SeedCategoryAsync("Work");
        var home = await SeedCategoryAsync("Home");
        await SeedTaskAsync(work.Id, title: "Work task");
        await SeedTaskAsync(home.Id, title: "Home task");

        var titles = await GetTitlesAsync($"categoryId={home.Id}");

        titles.ShouldBe(["Home task"]);
    }

    [Fact] // BR-L03
    public async Task List_FilterByStatus_ReturnsOnlyThatStatus()
    {
        var work = await SeedCategoryAsync();
        await SeedTaskAsync(work.Id, title: "Waiting");
        await SeedTaskAsync(work.Id, title: "Started", status: TaskStatus.InProgress);
        await SeedTaskAsync(work.Id, title: "Finished", status: TaskStatus.Done);

        var titles = await GetTitlesAsync("status=InProgress");

        titles.ShouldBe(["Started"]);
    }

    [Fact] // BR-L03
    public async Task List_FilterByPriority_ReturnsOnlyThatPriority()
    {
        var work = await SeedCategoryAsync();
        await SeedTaskAsync(work.Id, title: "Urgent", priority: TaskPriority.High);
        await SeedTaskAsync(work.Id, title: "Someday", priority: TaskPriority.Low);

        var titles = await GetTitlesAsync("priority=High");

        titles.ShouldBe(["Urgent"]);
    }

    [Fact] // BR-L03
    public async Task List_FilterByDueFromAndDueTo_IsInclusive_AndExcludesTasksWithoutDueDate()
    {
        var work = await SeedCategoryAsync();
        var today = Clock.Today;
        await SeedTaskAsync(work.Id, title: "Today", dueDate: today);
        await SeedTaskAsync(work.Id, title: "In two days", dueDate: today.AddDays(2));
        await SeedTaskAsync(work.Id, title: "In five days", dueDate: today.AddDays(5));
        await SeedTaskAsync(work.Id, title: "No date");

        var from = await GetTitlesAsync($"dueFrom={today.AddDays(2):yyyy-MM-dd}&sortBy=title&sortDirection=asc");
        var to = await GetTitlesAsync($"dueTo={today.AddDays(2):yyyy-MM-dd}&sortBy=title&sortDirection=asc");

        from.ShouldBe(["In five days", "In two days"]);
        to.ShouldBe(["In two days", "Today"]);
    }

    [Fact] // BR-L03
    public async Task List_FilterOverdue_UsesApplicationClock()
    {
        var work = await SeedCategoryAsync();
        var today = Clock.Today;
        await SeedTaskAsync(work.Id, title: "Due today", dueDate: today);
        await SeedTaskAsync(work.Id, title: "Due tomorrow", dueDate: today.AddDays(1));
        await SeedTaskAsync(work.Id, title: "Done", status: TaskStatus.Done, dueDate: today);
        await SeedTaskAsync(work.Id, title: "No date");

        (await GetTitlesAsync("overdue=true")).ShouldBeEmpty();

        // The database clock stays the same; only the TimeProvider moves.
        Clock.Advance(TimeSpan.FromDays(1));
        var page = await GetPageAsync("overdue=true");

        page.Items.Select(item => item.Title).ShouldBe(["Due today"]);
        page.Items[0].IsOverdue.ShouldBeTrue();
    }

    [Fact] // BR-L03
    public async Task List_Search_MatchesTitleOrDescriptionIgnoringCase()
    {
        var work = await SeedCategoryAsync();
        await SeedTaskAsync(work.Id, title: "Read BOOK", createdAt: Clock.GetUtcNow().AddMinutes(-2));
        await SeedTaskAsync(work.Id, title: "Write post", description: "About a Book review", createdAt: Clock.GetUtcNow().AddMinutes(-1));
        await SeedTaskAsync(work.Id, title: "Go running", description: "Park");

        var titles = await GetTitlesAsync("search=book");

        titles.ShouldBe(["Write post", "Read BOOK"]);
    }

    [Fact] // BR-L03
    public async Task List_CombinedFilters_AreAppliedWithAnd()
    {
        var work = await SeedCategoryAsync("Work");
        var home = await SeedCategoryAsync("Home");
        var today = Clock.Today;
        await SeedTaskAsync(work.Id, title: "Match report", priority: TaskPriority.High, dueDate: today.AddDays(1));
        await SeedTaskAsync(work.Id, title: "Report low", priority: TaskPriority.Low, dueDate: today.AddDays(1));
        await SeedTaskAsync(work.Id, title: "Report done", status: TaskStatus.Done, priority: TaskPriority.High, dueDate: today.AddDays(1));
        await SeedTaskAsync(work.Id, title: "Report far", priority: TaskPriority.High, dueDate: today.AddDays(30));
        await SeedTaskAsync(work.Id, title: "Other", priority: TaskPriority.High, dueDate: today.AddDays(1));
        await SeedTaskAsync(home.Id, title: "Home report", priority: TaskPriority.High, dueDate: today.AddDays(1));

        var titles = await GetTitlesAsync(
            $"categoryId={work.Id}&status=Pending&priority=High&dueFrom={today:yyyy-MM-dd}&dueTo={today.AddDays(7):yyyy-MM-dd}&search=REPORT");

        titles.ShouldBe(["Match report"]);
    }

    [Theory] // BR-L05
    [InlineData("asc", new[] { "Old", "Mid", "New" })]
    [InlineData("desc", new[] { "New", "Mid", "Old" })]
    public async Task List_SortByCreatedAt_SortsInDirection(string direction, string[] expected)
    {
        var work = await SeedCategoryAsync();
        await SeedTaskAsync(work.Id, title: "Mid", createdAt: Clock.GetUtcNow().AddHours(-1));
        await SeedTaskAsync(work.Id, title: "New");
        await SeedTaskAsync(work.Id, title: "Old", createdAt: Clock.GetUtcNow().AddHours(-2));

        var titles = await GetTitlesAsync($"sortBy=createdAt&sortDirection={direction}");

        titles.ShouldBe(expected);
    }

    [Theory] // BR-L05
    [InlineData("asc", new[] { "Soon", "Later", "No date A", "No date B" })]
    [InlineData("desc", new[] { "Later", "Soon", "No date A", "No date B" })]
    public async Task List_SortByDueDate_PutsTasksWithoutDueDateLast(string direction, string[] expected)
    {
        var work = await SeedCategoryAsync();
        var a = await SeedTaskAsync(work.Id, title: "No date A");
        var b = await SeedTaskAsync(work.Id, title: "No date B");
        await SeedTaskAsync(work.Id, title: "Later", dueDate: Clock.Today.AddDays(5));
        await SeedTaskAsync(work.Id, title: "Soon", dueDate: Clock.Today.AddDays(1));

        var titles = await GetTitlesAsync($"sortBy=dueDate&sortDirection={direction}");

        // Tasks without due date are tied; the id is the tiebreaker.
        var undated = a.Id.CompareTo(b.Id) < 0 ? new[] { "No date A", "No date B" } : ["No date B", "No date A"];
        titles.ShouldBe([.. expected[..2], .. undated]);
    }

    [Theory] // BR-L05
    [InlineData("asc", new[] { "Low", "Medium", "High" })]
    [InlineData("desc", new[] { "High", "Medium", "Low" })]
    public async Task List_SortByPriority_SortsByRankNotByName(string direction, string[] expected)
    {
        var work = await SeedCategoryAsync();
        await SeedTaskAsync(work.Id, title: "Medium", priority: TaskPriority.Medium);
        await SeedTaskAsync(work.Id, title: "High", priority: TaskPriority.High);
        await SeedTaskAsync(work.Id, title: "Low", priority: TaskPriority.Low);

        var titles = await GetTitlesAsync($"sortBy=priority&sortDirection={direction}");

        titles.ShouldBe(expected);
    }

    [Theory] // BR-L05
    [InlineData("asc", new[] { "Alpha", "Bravo", "Charlie" })]
    [InlineData("desc", new[] { "Charlie", "Bravo", "Alpha" })]
    public async Task List_SortByTitle_SortsAlphabetically(string direction, string[] expected)
    {
        var work = await SeedCategoryAsync();
        await SeedTaskAsync(work.Id, title: "Bravo");
        await SeedTaskAsync(work.Id, title: "Charlie");
        await SeedTaskAsync(work.Id, title: "Alpha");

        var titles = await GetTitlesAsync($"sortBy=title&sortDirection={direction}");

        titles.ShouldBe(expected);
    }

    [Fact] // BR-L01, BR-L02
    public async Task List_SecondPage_ReturnsPageAndMetadata()
    {
        var work = await SeedCategoryAsync();
        foreach (var title in new[] { "Task A", "Task B", "Task C", "Task D", "Task E" })
        {
            await SeedTaskAsync(work.Id, title: title);
        }

        var page = await GetPageAsync("sortBy=title&sortDirection=asc&page=2&pageSize=2");

        page.Items.Select(item => item.Title).ShouldBe(["Task C", "Task D"]);
        page.Page.ShouldBe(2);
        page.PageSize.ShouldBe(2);
        page.TotalCount.ShouldBe(5);
        page.TotalPages.ShouldBe(3);
    }

    [Theory] // BR-L01, BR-L04, BR-L05
    [InlineData("dueFrom=2026-11-02&dueTo=2026-11-01", "dueTo")]
    [InlineData("sortBy=status", "sortBy")]
    [InlineData("sortDirection=up", "sortDirection")]
    [InlineData("pageSize=101", "pageSize")]
    [InlineData("page=0", "page")]
    public async Task List_InvalidParameters_Returns400(string queryString, string field)
    {
        var response = await Client.GetAsync(new Uri($"/api/v1/tasks?{queryString}", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(400);
        problem.Code.ShouldBe("Validation.Failed");
        problem.Errors.ShouldNotBeNull().Keys.ShouldBe([field]);
    }

    private async Task<PagedResult<TaskResponse>> GetPageAsync(string queryString)
    {
        var response = await Client.GetAsync(new Uri($"/api/v1/tasks?{queryString}", UriKind.Relative), CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await ReadAsync<PagedResult<TaskResponse>>(response);
    }

    private async Task<string[]> GetTitlesAsync(string queryString) =>
        (await GetPageAsync(queryString)).Items.Select(item => item.Title).ToArray();
}
