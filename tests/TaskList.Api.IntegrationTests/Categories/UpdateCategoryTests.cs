using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Domain.Categories;

namespace TaskList.Api.IntegrationTests.Categories;

public sealed class UpdateCategoryTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Put_WithValidBody_Returns204AndUpdatesCategory()
    {
        var category = await SeedCategoryAsync("Work", "Office", "#000000");
        Clock.Advance(TimeSpan.FromHours(1));

        var response = await PutAsync($"/api/v1/categories/{category.Id}", new { name = " Job ", description = "  ", color = "#abcdef" });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await LoadAsync(category.Id);
        stored.Name.ShouldBe("Job");
        stored.Description.ShouldBeNull(); // BR-G01
        stored.Color.ShouldBe("#ABCDEF"); // BR-C03
        stored.CreatedAt.ShouldBe(category.CreatedAt);
        stored.UpdatedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Fact] // BR-C02
    public async Task Put_RenamingToOwnNameInOtherCasing_Returns204()
    {
        var category = await SeedCategoryAsync("Work");

        var response = await PutAsync($"/api/v1/categories/{category.Id}", new { name = "WORK" });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await LoadAsync(category.Id)).Name.ShouldBe("WORK");
    }

    [Fact]
    public async Task Put_WhenCategoryDoesNotExist_Returns404()
    {
        var response = await PutAsync($"/api/v1/categories/{Guid.NewGuid()}", new { name = "Work" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(404);
        problem.Code.ShouldBe("Category.NotFound");
    }

    [Fact] // BR-C01
    public async Task Put_WithNameOfAnotherCategory_Returns409()
    {
        await SeedCategoryAsync("Home");
        var category = await SeedCategoryAsync("Work");

        var response = await PutAsync($"/api/v1/categories/{category.Id}", new { name = "home" });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(409);
        problem.Code.ShouldBe("Category.NameAlreadyExists");
        (await LoadAsync(category.Id)).Name.ShouldBe("Work");
    }

    [Fact]
    public async Task Put_WithInvalidBody_Returns400WithErrors()
    {
        var category = await SeedCategoryAsync("Work");

        var response = await PutAsync($"/api/v1/categories/{category.Id}", new { name = "", color = "blue" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Code.ShouldBe("Validation.Failed");
        problem.Errors.ShouldNotBeNull().Keys.ShouldBe(["name", "color"], ignoreOrder: true);
    }

    private Task<Category> LoadAsync(Guid id) =>
        ExecuteDbContextAsync(db => db.Categories.SingleAsync(c => c.Id == id, CancellationToken));
}
