using Microsoft.Extensions.DependencyInjection;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Categories;
using TaskList.Application.Tasks;

namespace TaskList.Api.IntegrationTests.Persistence;

/// <summary>Proves the shared Application projections translate to SQL on PostgreSQL.</summary>
public sealed class ReadDbContextTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Fact] // BR-C07
    public async Task CategoryProjection_CountsTasksInSql()
    {
        var work = await SeedCategoryAsync("Work");
        await SeedCategoryAsync("Empty");
        await SeedTaskAsync(work.Id);
        await SeedTaskAsync(work.Id, status: TaskStatus.InProgress);
        await SeedTaskAsync(work.Id, status: TaskStatus.Done);

        var responses = await WithReadDb(db => db.ToListAsync(
            db.Categories.OrderBy(c => c.Name).SelectResponse(db.Tasks),
            CancellationToken));

        responses.Select(r => (r.Name, r.TaskCount, r.PendingTaskCount)).ShouldBe([("Empty", 0, 0), ("Work", 3, 2)]);
    }

    [Fact]
    public async Task TaskProjection_KeepsOrder_EmbedsCategory_AndCalculatesOverdue()
    {
        var study = await SeedCategoryAsync("Study", color: "#1E90FF");
        var past = Clock.GetUtcNow().AddDays(-10);
        await SeedTaskAsync(study.Id, title: "B overdue", dueDate: Clock.Today.AddDays(-1), createdAt: past);
        await SeedTaskAsync(study.Id, title: "A on time", dueDate: Clock.Today);
        await SeedTaskAsync(study.Id, title: "C done", status: TaskStatus.Done, dueDate: Clock.Today.AddDays(-2), createdAt: past);

        var responses = await WithReadDb(db => db.ToListAsync(
            db.Tasks.OrderByDescending(t => t.Title).SelectResponse(db.Categories, Clock.Today),
            CancellationToken));

        responses.Select(r => (r.Title, r.IsOverdue)).ShouldBe([("C done", false), ("B overdue", true), ("A on time", false)]);
        responses[0].Category.ShouldBe(new CategorySummary(study.Id, "Study", "#1E90FF"));
    }

    [Fact]
    public async Task FirstOrDefaultAndCount_Work()
    {
        var work = await SeedCategoryAsync("Work");

        var found = await WithReadDb(db => db.FirstOrDefaultAsync(db.Categories.Where(c => c.Id == work.Id).SelectResponse(db.Tasks), CancellationToken));
        var count = await WithReadDb(db => db.CountAsync(db.Categories, CancellationToken));

        found.ShouldNotBeNull().Name.ShouldBe("Work");
        count.ShouldBe(1);
    }

    private async Task<T> WithReadDb<T>(Func<IReadDbContext, Task<T>> query)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<IReadDbContext>());
    }
}
