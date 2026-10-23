using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Api.Contracts;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Domain.Tasks;

namespace TaskList.Api.IntegrationTests.Tasks;

public sealed class CreateTaskTests(ApiFactory factory) : IntegrationTest(factory)
{
    private const string Url = "/api/v1/tasks";

    [Fact] // BR-T02, BR-T03, BR-G01
    public async Task Create_ValidRequest_Returns201AndStoresPendingTaskWithDefaultPriority()
    {
        var category = await SeedCategoryAsync();

        var response = await PostAsync(Url, new
        {
            title = "  Read book  ",
            description = "  Chapter 1 ",
            categoryId = category.Id,
            dueDate = Clock.Today,
            status = "Done", // BR-T02: ignored
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await ReadAsync<CreatedResponse>(response);
        response.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe($"/api/v1/tasks/{body.Id}");

        var stored = await ExecuteDbContextAsync(db => db.Tasks.SingleAsync(task => task.Id == body.Id, CancellationToken));
        stored.Title.ShouldBe("Read book");
        stored.Description.ShouldBe("Chapter 1");
        stored.CategoryId.ShouldBe(category.Id);
        stored.Status.ShouldBe(TaskStatus.Pending);
        stored.Priority.ShouldBe(TaskPriority.Medium);
        stored.DueDate.ShouldBe(Clock.Today);
        stored.CompletedAt.ShouldBeNull();
        stored.CreatedAt.ShouldBe(Clock.UtcNow);
    }

    [Fact]
    public async Task Create_WithPriority_StoresPriority()
    {
        var category = await SeedCategoryAsync();

        var response = await PostAsync(Url, new { title = "Read book", categoryId = category.Id, priority = "High" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await ReadAsync<CreatedResponse>(response);
        var stored = await ExecuteDbContextAsync(db => db.Tasks.SingleAsync(task => task.Id == body.Id, CancellationToken));
        stored.Priority.ShouldBe(TaskPriority.High);
    }

    [Fact]
    public async Task Create_InvalidRequest_Returns400ValidationFailed()
    {
        var response = await PostAsync(Url, new { title = "  ab ", categoryId = Guid.Empty });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(400);
        problem.Code.ShouldBe("Validation.Failed");
        problem.Errors.ShouldNotBeNull().Keys.ShouldBe(["title", "categoryId"], ignoreOrder: true);
        (await ExecuteDbContextAsync(db => db.Tasks.CountAsync(CancellationToken))).ShouldBe(0);
    }

    [Fact] // BR-T04
    public async Task Create_DueDateInPast_Returns400DueDateInPast()
    {
        var category = await SeedCategoryAsync();

        var response = await PostAsync(Url, new { title = "Read book", categoryId = category.Id, dueDate = Clock.Today.AddDays(-1) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(400);
        problem.Code.ShouldBe("Task.DueDateInPast");
        (await ExecuteDbContextAsync(db => db.Tasks.CountAsync(CancellationToken))).ShouldBe(0);
    }

    [Fact] // BR-T01
    public async Task Create_UnknownCategory_Returns404CategoryNotFound()
    {
        var response = await PostAsync(Url, new { title = "Read book", categoryId = Guid.NewGuid() });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Category.NotFound");
    }
}
