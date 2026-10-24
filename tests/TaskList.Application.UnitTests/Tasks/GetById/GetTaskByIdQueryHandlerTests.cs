using Microsoft.Extensions.Time.Testing;
using TaskList.Application.Tasks;
using TaskList.Application.Tasks.GetById;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.GetById;

public sealed class GetTaskByIdQueryHandlerTests
{
    private readonly InMemoryReadDbContext _db = new();
    private readonly FakeTimeProvider _time = new(TestData.Now);

    [Fact]
    public async Task Handle_ExistingTask_ReturnsResponseWithCategory()
    {
        var category = TestData.Category("Study", "#1E90FF");
        var task = TestData.Task(category.Id, title: "Read book", priority: TaskPriority.High, dueDate: TestData.Today);
        _db.CategoryList.Add(category);
        _db.TaskList.AddRange([task, TestData.Task(category.Id, title: "Other")]);

        var result = await Handler().Handle(new GetTaskByIdQuery(task.Id), TestContext.Current.CancellationToken);

        var response = result.Value;
        response.Id.ShouldBe(task.Id);
        response.Title.ShouldBe("Read book");
        response.Priority.ShouldBe(TaskPriority.High);
        response.Category.ShouldBe(new CategorySummary(category.Id, "Study", "#1E90FF"));
        response.IsOverdue.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_DueDatePassed_IsOverdueUsingTimeProvider()
    {
        var category = TestData.Category();
        var task = TestData.Task(category.Id, dueDate: TestData.Today);
        _db.CategoryList.Add(category);
        _db.TaskList.Add(task);
        _time.Advance(TimeSpan.FromDays(1));

        var result = await Handler().Handle(new GetTaskByIdQuery(task.Id), TestContext.Current.CancellationToken);

        result.Value.IsOverdue.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_UnknownTask_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        var result = await Handler().Handle(new GetTaskByIdQuery(id), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(TaskErrors.NotFound(id));
    }

    private GetTaskByIdQueryHandler Handler() => new(_db, _time);
}
