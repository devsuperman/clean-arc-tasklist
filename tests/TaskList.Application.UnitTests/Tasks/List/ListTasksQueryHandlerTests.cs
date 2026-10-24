using Microsoft.Extensions.Time.Testing;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks;
using TaskList.Application.Tasks.List;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.List;

public sealed class ListTasksQueryHandlerTests
{
    private readonly InMemoryReadDbContext _db = new();
    private readonly FakeTimeProvider _time = new(TestData.Now);
    private readonly Guid _work;
    private readonly Guid _home;

    public ListTasksQueryHandlerTests()
    {
        var work = TestData.Category("Work");
        var home = TestData.Category("Home");
        _db.CategoryList.AddRange([work, home]);
        _work = work.Id;
        _home = home.Id;
    }

    [Fact] // BR-L05
    public async Task Handle_WithoutSort_SortsByCreatedAtDescending()
    {
        Add(TestData.Task(_work, title: "Old", createdAt: TestData.Now.AddDays(-2)));
        Add(TestData.Task(_work, title: "New", createdAt: TestData.Now));
        Add(TestData.Task(_work, title: "Mid", createdAt: TestData.Now.AddDays(-1)));

        var titles = await Titles(new ListTasksQuery());

        titles.ShouldBe(["New", "Mid", "Old"]);
    }

    [Theory] // BR-L03
    [InlineData(TaskStatus.InProgress, "Started")]
    [InlineData(TaskStatus.Done, "Finished")]
    public async Task Handle_FilterByStatus_ReturnsOnlyThatStatus(TaskStatus status, string expected)
    {
        Add(TestData.Task(_work, title: "Waiting"));
        Add(TestData.Task(_work, title: "Started", status: TaskStatus.InProgress));
        Add(TestData.Task(_work, title: "Finished", status: TaskStatus.Done));

        var titles = await Titles(new ListTasksQuery(Status: status));

        titles.ShouldBe([expected]);
    }

    [Fact] // BR-L03
    public async Task Handle_FilterByCategoryAndPriority_CombinesWithAnd()
    {
        Add(TestData.Task(_work, title: "Work high", priority: TaskPriority.High));
        Add(TestData.Task(_work, title: "Work low", priority: TaskPriority.Low));
        Add(TestData.Task(_home, title: "Home high", priority: TaskPriority.High));

        var titles = await Titles(new ListTasksQuery(CategoryId: _work, Priority: TaskPriority.High));

        titles.ShouldBe(["Work high"]);
    }

    [Fact] // BR-L03
    public async Task Handle_FilterByDueRange_IsInclusiveAndExcludesTasksWithoutDueDate()
    {
        Add(TestData.Task(_work, title: "Today", dueDate: TestData.Today));
        Add(TestData.Task(_work, title: "In 2 days", dueDate: TestData.Today.AddDays(2)));
        Add(TestData.Task(_work, title: "In 5 days", dueDate: TestData.Today.AddDays(5)));
        Add(TestData.Task(_work, title: "No date"));

        var titles = await Titles(new ListTasksQuery(
            DueFrom: TestData.Today,
            DueTo: TestData.Today.AddDays(2),
            SortBy: TaskSort.DueDate,
            SortDirection: TaskSort.Ascending));

        titles.ShouldBe(["Today", "In 2 days"]);
    }

    [Fact] // BR-L03
    public async Task Handle_FilterOverdue_UsesTimeProviderToday()
    {
        Add(TestData.Task(_work, title: "Due today", dueDate: TestData.Today));
        Add(TestData.Task(_work, title: "Done", status: TaskStatus.Done, dueDate: TestData.Today));
        Add(TestData.Task(_work, title: "Later", dueDate: TestData.Today.AddDays(3)));
        _time.Advance(TimeSpan.FromDays(1));

        var page = await Handle(new ListTasksQuery(Overdue: true));

        page.Items.Select(item => item.Title).ShouldBe(["Due today"]);
        page.Items[0].IsOverdue.ShouldBeTrue();
    }

    [Theory] // BR-L03
    [InlineData("READ")]
    [InlineData("chapter")]
    [InlineData("  read  ")]
    public async Task Handle_Search_MatchesTitleOrDescriptionIgnoringCase(string search)
    {
        Add(TaskItem.Create("Read book", "First chapter", _work, null, null, TestData.Now).Value);
        Add(TaskItem.Create("Write post", null, _work, null, null, TestData.Now).Value);

        var titles = await Titles(new ListTasksQuery(Search: search));

        titles.ShouldBe(["Read book"]);
    }

    [Fact] // BR-L03
    public async Task Handle_BlankSearch_IsIgnored()
    {
        Add(TestData.Task(_work, title: "One"));
        Add(TestData.Task(_work, title: "Two"));

        var page = await Handle(new ListTasksQuery(Search: "   "));

        page.TotalCount.ShouldBe(2);
    }

    [Theory] // BR-L05
    [InlineData("asc", new[] { "Low", "Medium", "High" })]
    [InlineData("desc", new[] { "High", "Medium", "Low" })]
    public async Task Handle_SortByPriority_SortsByRank(string direction, string[] expected)
    {
        Add(TestData.Task(_work, title: "Medium", priority: TaskPriority.Medium));
        Add(TestData.Task(_work, title: "High", priority: TaskPriority.High));
        Add(TestData.Task(_work, title: "Low", priority: TaskPriority.Low));

        var titles = await Titles(new ListTasksQuery(SortBy: "priority", SortDirection: direction));

        titles.ShouldBe(expected);
    }

    [Theory] // BR-L05
    [InlineData("asc", new[] { "Soon", "Later", "No date" })]
    [InlineData("desc", new[] { "Later", "Soon", "No date" })]
    public async Task Handle_SortByDueDate_PutsTasksWithoutDueDateLast(string direction, string[] expected)
    {
        Add(TestData.Task(_work, title: "No date"));
        Add(TestData.Task(_work, title: "Later", dueDate: TestData.Today.AddDays(5)));
        Add(TestData.Task(_work, title: "Soon", dueDate: TestData.Today.AddDays(1)));

        var titles = await Titles(new ListTasksQuery(SortBy: "DUEDATE", SortDirection: direction));

        titles.ShouldBe(expected);
    }

    [Fact] // BR-L05
    public async Task Handle_SortByTitleAscending_SortsAlphabetically()
    {
        Add(TestData.Task(_work, title: "Charlie"));
        Add(TestData.Task(_work, title: "Alpha"));
        Add(TestData.Task(_work, title: "Bravo"));

        var titles = await Titles(new ListTasksQuery(SortBy: TaskSort.Title, SortDirection: TaskSort.Ascending));

        titles.ShouldBe(["Alpha", "Bravo", "Charlie"]);
    }

    [Fact] // BR-L01, BR-L02
    public async Task Handle_Paging_ReturnsRequestedPageAndMetadata()
    {
        for (var i = 1; i <= 5; i++)
        {
            Add(TestData.Task(_work, title: $"Task {i}", createdAt: TestData.Now.AddMinutes(i)));
        }

        var page = await Handle(new ListTasksQuery(Page: 2, PageSize: 2));

        page.Items.Select(item => item.Title).ShouldBe(["Task 3", "Task 2"]);
        page.TotalCount.ShouldBe(5);
        page.TotalPages.ShouldBe(3);
    }

    private void Add(TaskItem task) => _db.TaskList.Add(task);

    private async Task<PagedResult<TaskResponse>> Handle(ListTasksQuery query)
    {
        var handler = new ListTasksQueryHandler(_db, _time);
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);
        return result.Value;
    }

    private async Task<string[]> Titles(ListTasksQuery query) =>
        (await Handle(query)).Items.Select(item => item.Title).ToArray();
}
