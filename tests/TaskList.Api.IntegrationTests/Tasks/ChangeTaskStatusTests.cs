using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Domain.Tasks;

namespace TaskList.Api.IntegrationTests.Tasks;

public sealed class ChangeTaskStatusTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Theory] // BR-T05
    [InlineData("start", TaskStatus.Pending, TaskStatus.InProgress)]
    [InlineData("complete", TaskStatus.Pending, TaskStatus.Done)]
    [InlineData("complete", TaskStatus.InProgress, TaskStatus.Done)]
    [InlineData("reopen", TaskStatus.InProgress, TaskStatus.Pending)]
    [InlineData("reopen", TaskStatus.Done, TaskStatus.Pending)]
    public async Task Action_AllowedTransition_Returns204AndStoresStatus(string action, TaskStatus from, TaskStatus expected)
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id, status: from);
        Clock.Advance(TimeSpan.FromHours(1));

        var response = await PostActionAsync(task.Id, action);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await LoadAsync(task.Id);
        stored.Status.ShouldBe(expected);
        stored.UpdatedAt.ShouldBe(Clock.UtcNow);
    }

    [Fact] // BR-T07
    public async Task Complete_SetsCompletedAt_AndReopen_ClearsIt()
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id, status: TaskStatus.InProgress);
        Clock.Advance(TimeSpan.FromHours(2));
        var completedAt = Clock.UtcNow;

        (await PostActionAsync(task.Id, "complete")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await LoadAsync(task.Id)).CompletedAt.ShouldBe(completedAt);

        Clock.Advance(TimeSpan.FromDays(1));
        (await PostActionAsync(task.Id, "reopen")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var reopened = await LoadAsync(task.Id);
        reopened.Status.ShouldBe(TaskStatus.Pending);
        reopened.CompletedAt.ShouldBeNull();
    }

    [Theory] // BR-T05
    [InlineData("start", TaskStatus.InProgress)]
    [InlineData("start", TaskStatus.Done)]
    [InlineData("complete", TaskStatus.Done)]
    [InlineData("reopen", TaskStatus.Pending)]
    public async Task Action_ForbiddenTransition_Returns409InvalidStatusTransition(string action, TaskStatus from)
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id, status: from);

        var response = await PostActionAsync(task.Id, action);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(409);
        problem.Code.ShouldBe("Task.InvalidStatusTransition");
        (await LoadAsync(task.Id)).Status.ShouldBe(from);
    }

    [Theory]
    [InlineData("start")]
    [InlineData("complete")]
    [InlineData("reopen")]
    public async Task Action_UnknownTask_Returns404TaskNotFound(string action)
    {
        var response = await PostActionAsync(Guid.NewGuid(), action);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Task.NotFound");
    }

    private Task<HttpResponseMessage> PostActionAsync(Guid id, string action) =>
        Client.PostAsync(new Uri($"/api/v1/tasks/{id}/{action}", UriKind.Relative), null, CancellationToken);

    private Task<TaskItem> LoadAsync(Guid id) =>
        ExecuteDbContextAsync(db => db.Tasks.AsNoTracking().SingleAsync(task => task.Id == id, CancellationToken));
}
