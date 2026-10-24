using TaskList.Application.Categories.Get;
using TaskList.Application.UnitTests.TestDoubles;

namespace TaskList.Application.UnitTests.Categories.Get;

public sealed class GetCategoryQueryHandlerTests
{
    private readonly InMemoryReadDbContext _db = new();

    private GetCategoryQueryHandler Handler => new(_db);

    [Fact] // BR-C07
    public async Task Handle_WhenCategoryExists_ReturnsItWithCounters()
    {
        var work = TestData.Category("Work", "#1E90FF");
        var other = TestData.Category("Home");
        _db.CategoryList.AddRange([work, other]);
        _db.TaskList.AddRange([
            TestData.Task(work.Id),
            TestData.Task(work.Id, status: TaskStatus.Done),
            TestData.Task(other.Id),
        ]);

        var result = await Handler.Handle(new GetCategoryQuery(work.Id), TestContext.Current.CancellationToken);

        var category = result.Value;
        category.Id.ShouldBe(work.Id);
        category.Name.ShouldBe("Work");
        category.Color.ShouldBe("#1E90FF");
        category.TaskCount.ShouldBe(2);
        category.PendingTaskCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var result = await Handler.Handle(new GetCategoryQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Category.NotFound");
    }
}
