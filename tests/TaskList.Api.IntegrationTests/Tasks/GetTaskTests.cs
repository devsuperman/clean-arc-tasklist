using System.Net;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Tasks;
using TaskList.Domain.Tasks;

namespace TaskList.Api.IntegrationTests.Tasks;

public sealed class GetTaskTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Get_ExistingTask_Returns200WithCategoryAndIsOverdue()
    {
        var category = await SeedCategoryAsync("Study", color: "#1E90FF");
        var task = await SeedTaskAsync(
            category.Id,
            title: "Read book",
            description: "Chapter 1",
            status: TaskStatus.InProgress,
            priority: TaskPriority.High,
            dueDate: Clock.Today.AddDays(1));

        var response = await Client.GetAsync(new Uri($"/api/v1/tasks/{task.Id}", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await ReadAsync<TaskResponse>(response);
        body.Id.ShouldBe(task.Id);
        body.Title.ShouldBe("Read book");
        body.Description.ShouldBe("Chapter 1");
        body.Status.ShouldBe(TaskStatus.InProgress);
        body.Priority.ShouldBe(TaskPriority.High);
        body.DueDate.ShouldBe(Clock.Today.AddDays(1));
        body.IsOverdue.ShouldBeFalse();
        body.Category.ShouldBe(new CategorySummary(category.Id, "Study", "#1E90FF"));
        body.CreatedAt.ShouldBe(task.CreatedAt);
    }

    [Fact]
    public async Task Get_AfterDueDatePasses_IsOverdue()
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id, dueDate: Clock.Today);
        var url = new Uri($"/api/v1/tasks/{task.Id}", UriKind.Relative);

        (await ReadAsync<TaskResponse>(await Client.GetAsync(url, CancellationToken))).IsOverdue.ShouldBeFalse();

        Clock.Advance(TimeSpan.FromDays(1));

        (await ReadAsync<TaskResponse>(await Client.GetAsync(url, CancellationToken))).IsOverdue.ShouldBeTrue();
    }

    [Fact]
    public async Task Get_UnknownTask_Returns404()
    {
        var response = await Client.GetAsync(new Uri($"/api/v1/tasks/{Guid.NewGuid()}", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Task.NotFound");
    }
}
