using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Domain.Tasks;

namespace TaskList.Api.IntegrationTests.Tasks;

public sealed class DeleteTaskTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Theory] // BR-T09
    [InlineData(TaskStatus.Pending)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Done)]
    public async Task Delete_ExistingTaskInAnyStatus_Returns204AndRemovesIt(TaskStatus status)
    {
        var category = await SeedCategoryAsync();
        var task = await SeedTaskAsync(category.Id, status: status);
        var other = await SeedTaskAsync(category.Id, title: "Keep me");

        var response = await Client.DeleteAsync(new Uri($"/api/v1/tasks/{task.Id}", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var remaining = await ExecuteDbContextAsync(db => db.Tasks.Select(t => t.Id).ToListAsync(CancellationToken));
        remaining.ShouldBe([other.Id]);
    }

    [Fact]
    public async Task Delete_UnknownTask_Returns404()
    {
        var response = await Client.DeleteAsync(new Uri($"/api/v1/tasks/{Guid.NewGuid()}", UriKind.Relative), CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Task.NotFound");
    }
}
