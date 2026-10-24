using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskList.Api.IntegrationTests.Infrastructure;
using TaskList.Application.Abstractions.Data;
using TaskList.Domain.Categories;

namespace TaskList.Api.IntegrationTests.Persistence;

public sealed class RepositoryTests(ApiFactory factory) : IntegrationTest(factory)
{
    [Theory] // BR-C01, BR-C02
    [InlineData("Work", false, true)]
    [InlineData("WORK", false, true)]
    [InlineData("  work ", false, true)]
    [InlineData("Home", false, false)]
    [InlineData("work", true, false)]
    public async Task NameExistsAsync_IgnoresCase_AndCanExcludeItself(string name, bool excludeSelf, bool expected)
    {
        var work = await SeedCategoryAsync("Work");

        var exists = await WithRepository<ICategoryRepository, bool>(repository =>
            repository.NameExistsAsync(name, excludeSelf ? work.Id : null, CancellationToken));

        exists.ShouldBe(expected);
    }

    [Fact]
    public async Task CategoryRepository_AddGetExistsRemove()
    {
        var category = Category.Create("Study", null, "#00FF00", Clock.GetUtcNow()).Value;

        await WithScope(async provider =>
        {
            await provider.GetRequiredService<ICategoryRepository>().AddAsync(category, CancellationToken);
            await provider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken);
        });

        (await WithRepository<ICategoryRepository, bool>(r => r.ExistsAsync(category.Id, CancellationToken))).ShouldBeTrue();
        (await WithRepository<ICategoryRepository, bool>(r => r.ExistsAsync(Guid.NewGuid(), CancellationToken))).ShouldBeFalse();

        await WithScope(async provider =>
        {
            var repository = provider.GetRequiredService<ICategoryRepository>();
            var stored = await repository.GetByIdAsync(category.Id, CancellationToken);
            stored.ShouldNotBeNull().Color.ShouldBe("#00FF00");
            repository.Remove(stored);
            await provider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken);
        });

        (await WithRepository<ICategoryRepository, Category?>(r => r.GetByIdAsync(category.Id, CancellationToken))).ShouldBeNull();
    }

    [Fact]
    public async Task TaskRepository_CountsByCategory_AndRoundTripsFields()
    {
        var work = await SeedCategoryAsync("Work");
        var home = await SeedCategoryAsync("Home");
        var task = await SeedTaskAsync(work.Id, status: TaskStatus.Done, dueDate: Clock.Today.AddDays(5));
        await SeedTaskAsync(work.Id);
        await SeedTaskAsync(home.Id);

        (await WithRepository<ITaskRepository, int>(r => r.CountByCategoryAsync(work.Id, CancellationToken))).ShouldBe(2);

        var stored = await WithRepository<ITaskRepository, Domain.Tasks.TaskItem?>(r => r.GetByIdAsync(task.Id, CancellationToken));
        stored.ShouldNotBeNull();
        stored.Status.ShouldBe(TaskStatus.Done);
        stored.DueDate.ShouldBe(Clock.Today.AddDays(5));
        stored.CompletedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Fact] // BR-C04 at database level
    public async Task Database_RejectsDeletingCategoryWithTasks()
    {
        var work = await SeedCategoryAsync("Work");
        await SeedTaskAsync(work.Id);

        await Should.ThrowAsync<DbUpdateException>(() => ExecuteDbContextAsync(async db =>
        {
            db.Categories.Remove(await db.Categories.SingleAsync(CancellationToken));
            await db.SaveChangesAsync(CancellationToken);
        }));
    }

    [Fact] // BR-C01 at database level
    public async Task Database_RejectsDuplicateNameWithDifferentCase()
    {
        await SeedCategoryAsync("Work");

        await Should.ThrowAsync<DbUpdateException>(() => SeedCategoryAsync("work"));
    }

    private async Task WithScope(Func<IServiceProvider, Task> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    private async Task<TResult> WithRepository<TRepository, TResult>(Func<TRepository, Task<TResult>> action)
        where TRepository : notnull
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TRepository>());
    }
}
