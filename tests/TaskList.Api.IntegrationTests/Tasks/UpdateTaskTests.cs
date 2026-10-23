using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Domain.Tasks;

namespace TaskList.Api.IntegrationTests.Tasks;

public sealed class UpdateTaskTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // BR-T06, BR-T10, BR-G01
    public async Task Update_ValidRequest_Returns204AndStoresChanges()
    {
        var work = await SeedCategoryAsync("Work");
        var home = await SeedCategoryAsync("Home");
        var task = await SeedTaskAsync(work.Id, priority: TaskPriority.Low);
        Clock.Advance(TimeSpan.FromHours(1));

        var response = await PutAsync($"/api/v1/tasks/{task.Id}", new
        {
            title = "  Write report ",
            description = " Quarterly ",
            categoryId = home.Id,
            priority = "High",
            dueDate = Clock.Today.AddDays(2),
            status = "Done", // BR-T06: ignored
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await LoadAsync(task.Id);
        stored.Title.ShouldBe("Write report");
        stored.Description.ShouldBe("Quarterly");
        stored.CategoryId.ShouldBe(home.Id);
        stored.Priority.ShouldBe(TaskPriority.High);
        stored.DueDate.ShouldBe(Clock.Today.AddDays(2));
        stored.Status.ShouldBe(TaskStatus.Pending);
        stored.UpdatedAt.ShouldBe(Clock.UtcNow);
    }

    [Fact] // BR-T04
    public async Task Update_UnchangedPastDueDate_Returns204()
    {
        var category = await SeedCategoryAsync();
        var pastDue = Clock.Today.AddDays(-3);
        var task = await SeedTaskAsync(category.Id, dueDate: pastDue, createdAt: Clock.UtcNow.AddDays(-10));

        var response = await PutAsync($"/api/v1/tasks/{task.Id}", new { title = "Renamed", categoryId = category.Id, dueDate = pastDue });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await LoadAsync(task.Id);
        stored.Title.ShouldBe("Renamed");
        stored.DueDate.ShouldBe(pastDue);
    }

    [Fact] // BR-T04
    public async Task Update_NewPastDueDate_Returns400DueDateInPast()
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id);

        var response = await PutAsync($"/api/v1/tasks/{task.Id}", new { title = "Renamed", categoryId = category.Id, dueDate = Clock.Today.AddDays(-1) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadProblemAsync(response)).Code.ShouldBe("Task.DueDateInPast");
    }

    [Fact]
    public async Task Update_InvalidRequest_Returns400ValidationFailed()
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id);

        var response = await PutAsync($"/api/v1/tasks/{task.Id}", new { title = " ", categoryId = category.Id });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(400);
        problem.Code.ShouldBe("Validation.Failed");
        problem.Errors.ShouldNotBeNull().Keys.ShouldBe(["title"]);
    }

    [Fact]
    public async Task Update_UnknownTask_Returns404TaskNotFound()
    {
        var category = await SeedCategoryAsync();

        var response = await PutAsync($"/api/v1/tasks/{Guid.NewGuid()}", new { title = "Renamed", categoryId = category.Id });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Task.NotFound");
    }

    [Fact] // BR-T01
    public async Task Update_UnknownCategory_Returns404CategoryNotFound()
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id);

        var response = await PutAsync($"/api/v1/tasks/{task.Id}", new { title = "Renamed", categoryId = Guid.NewGuid() });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Category.NotFound");
        (await LoadAsync(task.Id)).CategoryId.ShouldBe(category.Id);
    }

    [Fact] // BR-T08
    public async Task Update_DoneTask_Returns409CompletedTaskIsReadOnly()
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id, status: TaskStatus.Done);

        var response = await PutAsync($"/api/v1/tasks/{task.Id}", new { title = "Renamed", categoryId = category.Id });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(409);
        problem.Code.ShouldBe("Task.CompletedTaskIsReadOnly");
        (await LoadAsync(task.Id)).Title.ShouldBe("Read book");
    }

    private Task<TaskItem> LoadAsync(Guid id) =>
        ExecuteDbContextAsync(db => db.Tasks.AsNoTracking().SingleAsync(task => task.Id == id, CancellationToken));
}
