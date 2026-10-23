using System.Net;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Categories;

namespace TaskList.Api.IntegrationTests.Categories;

public sealed class GetCategoryTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // BR-C07
    public async Task Get_WhenCategoryExists_Returns200WithCounters()
    {
        var work = await SeedCategoryAsync("Work", "Office", "#1E90FF");
        await SeedTaskAsync(work.Id);
        await SeedTaskAsync(work.Id, status: TaskStatus.InProgress);
        await SeedTaskAsync(work.Id, status: TaskStatus.Done);

        var response = await Client.GetAsync($"/api/v1/categories/{work.Id}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await ReadAsync<CategoryResponse>(response);
        body.ShouldBe(new CategoryResponse(work.Id, "Work", "Office", "#1E90FF", 3, 2, Clock.GetUtcNow(), null));
    }

    [Fact]
    public async Task Get_WhenCategoryDoesNotExist_Returns404()
    {
        var response = await Client.GetAsync($"/api/v1/categories/{Guid.NewGuid()}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Category.NotFound");
    }
}
