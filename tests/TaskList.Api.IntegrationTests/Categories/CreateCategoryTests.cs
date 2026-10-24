using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Api.Contracts;
using TaskList.Api.IntegrationTests.Infrastructure;

namespace TaskList.Api.IntegrationTests.Categories;

public sealed class CreateCategoryTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Post_WithValidBody_Returns201WithLocationAndStoresCategory()
    {
        var response = await PostAsync("/api/v1/categories", new { name = "  Work  ", description = "Office", color = "#1e90ff" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await ReadAsync<CreatedResponse>(response);
        response.Headers.Location.ShouldBe(new Uri($"/api/v1/categories/{body.Id}", UriKind.Relative));

        var stored = await ExecuteDbContextAsync(db => db.Categories.SingleAsync(c => c.Id == body.Id, CancellationToken));
        stored.Name.ShouldBe("Work"); // BR-G01
        stored.Description.ShouldBe("Office");
        stored.Color.ShouldBe("#1E90FF"); // BR-C03
        stored.CreatedAt.ShouldBe(Clock.GetUtcNow());
        stored.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Post_WithInvalidBody_Returns400WithErrors()
    {
        var response = await PostAsync("/api/v1/categories", new { name = " W ", description = new string('a', 201), color = "red" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(400);
        problem.Code.ShouldBe("Validation.Failed");
        problem.Errors.ShouldNotBeNull().Keys.ShouldBe(["name", "description", "color"], ignoreOrder: true);
    }

    [Fact] // BR-C01
    public async Task Post_WithExistingNameInOtherCasing_Returns409()
    {
        await SeedCategoryAsync("Work");

        var response = await PostAsync("/api/v1/categories", new { name = "WORK" });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ReadProblemAsync(response);
        problem.Status.ShouldBe(409);
        problem.Code.ShouldBe("Category.NameAlreadyExists");
        (await ExecuteDbContextAsync(db => db.Categories.CountAsync(CancellationToken))).ShouldBe(1);
    }
}
