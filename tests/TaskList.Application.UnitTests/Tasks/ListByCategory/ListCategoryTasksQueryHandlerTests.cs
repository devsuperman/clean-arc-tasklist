using Microsoft.Extensions.Time.Testing;
using TaskList.Application.Tasks.ListByCategory;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.ListByCategory;

public sealed class ListCategoryTasksQueryHandlerTests
{
    private readonly InMemoryReadDbContext _db = new();
    private readonly FakeTimeProvider _time = new(TestData.Now);

    [Fact] // UC-14
    public async Task Handle_ExistingCategory_ReturnsOnlyItsTasksWithFilters()
    {
        var work = TestData.Category("Work");
        var home = TestData.Category("Home");
        _db.CategoryList.AddRange([work, home]);
        _db.TaskList.AddRange([
            TestData.Task(work.Id, title: "Work high", priority: TaskPriority.High),
            TestData.Task(work.Id, title: "Work low", priority: TaskPriority.Low),
            TestData.Task(home.Id, title: "Home high", priority: TaskPriority.High),
        ]);

        var result = await Handler().Handle(
            new ListCategoryTasksQuery(work.Id, Priority: TaskPriority.High),
            TestContext.Current.CancellationToken);

        result.Value.Items.Select(item => item.Title).ShouldBe(["Work high"]);
        result.Value.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ExistingCategoryWithoutTasks_ReturnsEmptyPage()
    {
        var work = TestData.Category("Work");
        _db.CategoryList.Add(work);

        var result = await Handler().Handle(new ListCategoryTasksQuery(work.Id), TestContext.Current.CancellationToken);

        result.Value.Items.ShouldBeEmpty();
        result.Value.TotalPages.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_UnknownCategory_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        var result = await Handler().Handle(new ListCategoryTasksQuery(id), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(CategoryErrors.NotFound(id));
    }

    private ListCategoryTasksQueryHandler Handler() => new(_db, _time);
}
