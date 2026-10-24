using System.Net;
using TaskList.Api.Contracts;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Categories;
using TaskList.Application.Tasks;

namespace TaskList.Api.IntegrationTests.EndToEnd;

/// <summary>Flows that cross several use cases, using only the public API.</summary>
public sealed class EndToEndTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // BR-C04, BR-C05, BR-T09
    public async Task DeleteCategory_IsBlockedUntilItsTasksAreDeleted()
    {
        var categoryId = (await ReadAsync<CreatedResponse>(await PostAsync("/api/v1/categories", new { name = "Work" }))).Id;
        var firstTaskId = await CreateTaskAsync(categoryId, "First task");
        var secondTaskId = await CreateTaskAsync(categoryId, "Second task");

        var blocked = await Client.DeleteAsync($"/api/v1/categories/{categoryId}", CancellationToken);
        blocked.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadProblemAsync(blocked);
        problem.Code.ShouldBe("Category.HasTasks");
        problem.Detail.ShouldNotBeNull().ShouldContain("2");

        (await Client.DeleteAsync($"/api/v1/tasks/{firstTaskId}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await Client.DeleteAsync($"/api/v1/tasks/{secondTaskId}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await Client.DeleteAsync($"/api/v1/categories/{categoryId}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await Client.GetAsync($"/api/v1/categories/{categoryId}", CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact] // BR-G02, BR-T02: id, timestamps and status sent by the client are ignored
    public async Task Create_IgnoresSystemFieldsSentByTheClient()
    {
        var forgedId = Guid.NewGuid();
        var forgedTime = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var categoryResponse = await PostAsync("/api/v1/categories", new { id = forgedId, name = "Work", createdAt = forgedTime, updatedAt = forgedTime });
        var categoryId = (await ReadAsync<CreatedResponse>(categoryResponse)).Id;
        var taskResponse = await PostAsync("/api/v1/tasks", new
        {
            id = forgedId,
            title = "Read book",
            categoryId,
            status = "Done",
            createdAt = forgedTime,
            updatedAt = forgedTime,
            completedAt = forgedTime,
        });
        var taskId = (await ReadAsync<CreatedResponse>(taskResponse)).Id;

        var category = await ReadAsync<CategoryResponse>(await Client.GetAsync($"/api/v1/categories/{categoryId}", CancellationToken));
        var task = await ReadAsync<TaskResponse>(await Client.GetAsync($"/api/v1/tasks/{taskId}", CancellationToken));

        categoryId.ShouldNotBe(forgedId);
        category.CreatedAt.ShouldBe(Clock.UtcNow);
        category.UpdatedAt.ShouldBeNull();
        taskId.ShouldNotBe(forgedId);
        task.Status.ShouldBe(TaskStatus.Pending);
        task.CreatedAt.ShouldBe(Clock.UtcNow);
        task.UpdatedAt.ShouldBeNull();
        task.CompletedAt.ShouldBeNull();
    }

    [Fact] // BR-G03, BR-G04: every timestamp comes from the injected clock and is returned in UTC
    public async Task Timestamps_ComeFromTheClock_InUtc()
    {
        var created = new DateTimeOffset(2026, 11, 2, 8, 30, 0, TimeSpan.Zero);
        Clock.UtcNow = created;
        var categoryId = (await ReadAsync<CreatedResponse>(await PostAsync("/api/v1/categories", new { name = "Work" }))).Id;
        var taskId = await CreateTaskAsync(categoryId, "Read book");

        Clock.Advance(TimeSpan.FromHours(5));
        (await Client.PostAsync($"/api/v1/tasks/{taskId}/complete", null, CancellationToken)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var task = await ReadAsync<TaskResponse>(await Client.GetAsync($"/api/v1/tasks/{taskId}", CancellationToken));
        task.CreatedAt.ShouldBe(created);
        task.CreatedAt.Offset.ShouldBe(TimeSpan.Zero);
        task.CompletedAt.ShouldBe(created.AddHours(5));
        task.UpdatedAt.ShouldBe(created.AddHours(5));
    }

    private async Task<Guid> CreateTaskAsync(Guid categoryId, string title)
    {
        var response = await PostAsync("/api/v1/tasks", new { title, categoryId });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await ReadAsync<CreatedResponse>(response)).Id;
    }
}
