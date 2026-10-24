using System.Net;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Categories;
using TaskList.Application.Common.Paging;

namespace TaskList.Api.IntegrationTests.Categories;

public sealed class ListCategoriesTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // BR-L01, BR-L02, BR-C06
    public async Task List_WithPaging_Returns200WithPageOrderedByNameAndMetadata()
    {
        foreach (var name in new[] { "Work", "Home", "Study", "Errands", "Garden" })
        {
            await SeedCategoryAsync(name);
        }

        var response = await Client.GetAsync("/api/v1/categories?page=2&pageSize=2", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await ReadAsync<PagedResult<CategoryResponse>>(response);
        page.Items.Select(c => c.Name).ShouldBe(["Home", "Study"]);
        page.Page.ShouldBe(2);
        page.PageSize.ShouldBe(2);
        page.TotalCount.ShouldBe(5);
        page.TotalPages.ShouldBe(3);
    }

    [Fact] // BR-L01, BR-C07
    public async Task List_WithoutQueryString_UsesDefaultsAndReturnsCounters()
    {
        var work = await SeedCategoryAsync("Work");
        await SeedTaskAsync(work.Id);
        await SeedTaskAsync(work.Id, status: TaskStatus.Done);

        var response = await Client.GetAsync("/api/v1/categories", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await ReadAsync<PagedResult<CategoryResponse>>(response);
        page.Page.ShouldBe(1);
        page.PageSize.ShouldBe(20);
        page.Items.Single().ShouldBe(new CategoryResponse(work.Id, "Work", null, null, 2, 1, Clock.GetUtcNow(), null));
    }

    [Fact] // BR-L06
    public async Task List_WithSearch_FiltersByNameIgnoringCase()
    {
        await SeedCategoryAsync("Work");
        await SeedCategoryAsync("Homework");
        await SeedCategoryAsync("Home");

        var response = await Client.GetAsync("/api/v1/categories?search=WORK", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await ReadAsync<PagedResult<CategoryResponse>>(response);
        page.Items.Select(c => c.Name).ShouldBe(["Homework", "Work"]);
        page.TotalCount.ShouldBe(2);
    }

    [Theory] // BR-L01
    [InlineData("pageSize=101", "pageSize")]
    [InlineData("pageSize=0", "pageSize")]
    [InlineData("page=0", "page")]
    public async Task List_WithPagingOutOfRange_Returns400(string queryString, string field)
    {
        var response = await Client.GetAsync($"/api/v1/categories?{queryString}", CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(400);
        problem.Code.ShouldBe("Validation.Failed");
        problem.Errors.ShouldNotBeNull().Keys.ShouldBe([field]);
    }
}
