using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Api.IntegrationTests.Infrastructure;

namespace TaskList.Api.IntegrationTests.Categories;

public sealed class DeleteCategoryTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // BR-C05
    public async Task Delete_WhenCategoryIsEmpty_Returns204AndRemovesIt()
    {
        var category = await SeedCategoryAsync("Work");

        var response = await Client.DeleteAsync($"/api/v1/categories/{category.Id}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ExistsAsync(category.Id)).ShouldBeFalse();
    }

    [Fact]
    public async Task Delete_WhenCategoryDoesNotExist_Returns404()
    {
        var response = await Client.DeleteAsync($"/api/v1/categories/{Guid.NewGuid()}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Category.NotFound");
    }

    [Fact] // BR-C04
    public async Task Delete_WhenCategoryHasTasks_Returns409WithTaskCount()
    {
        var category = await SeedCategoryAsync("Work");
        await SeedTaskAsync(category.Id);
        await SeedTaskAsync(category.Id, status: TaskStatus.Done);

        var response = await Client.DeleteAsync($"/api/v1/categories/{category.Id}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(409);
        problem.Code.ShouldBe("Category.HasTasks");
        problem.Detail.ShouldNotBeNull().ShouldContain("2");
        (await ExistsAsync(category.Id)).ShouldBeTrue();
    }

    private Task<bool> ExistsAsync(Guid id) =>
        ExecuteDbContextAsync(db => db.Categories.AnyAsync(c => c.Id == id, CancellationToken));
}
