using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;
using TaskList.Infrastructure.Persistence;

namespace TaskList.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests. Each test starts with an empty database and the clock at
/// <see cref="TestTimeProvider.DefaultNow"/>. Seed data <b>directly through the database</b> with
/// <see cref="SeedCategoryAsync"/> / <see cref="SeedTaskAsync"/>, never through another use case's endpoint.
/// </summary>
public abstract class IntegrationTest(ApiFactory factory) : IAsyncLifetime
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    protected ApiFactory Factory { get; } = factory;

    protected HttpClient Client { get; } = factory.CreateClient();

    protected TestTimeProvider Clock => Factory.Clock;

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public virtual async ValueTask InitializeAsync() => await Factory.ResetAsync();

    public virtual ValueTask DisposeAsync()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    protected async Task<Category> SeedCategoryAsync(string name = "Work", string? description = null, string? color = null)
    {
        var category = Category.Create(name, description, color, Clock.GetUtcNow()).Value;
        await ExecuteDbContextAsync(async db =>
        {
            db.Categories.Add(category);
            await db.SaveChangesAsync(CancellationToken);
        });
        return category;
    }

    /// <summary>
    /// Seeds a task in the given status. <paramref name="createdAt"/> defaults to the clock; pass an earlier
    /// value to seed a task whose due date is now in the past.
    /// </summary>
    protected async Task<TaskItem> SeedTaskAsync(
        Guid categoryId,
        string title = "Read book",
        TaskStatus status = TaskStatus.Pending,
        TaskPriority priority = TaskPriority.Medium,
        DateOnly? dueDate = null,
        string? description = null,
        DateTimeOffset? createdAt = null)
    {
        var created = createdAt ?? Clock.GetUtcNow();
        var task = TaskItem.Create(title, description, categoryId, priority, dueDate, created).Value;
        _ = status switch
        {
            TaskStatus.InProgress => task.Start(created),
            TaskStatus.Done => task.Complete(created),
            _ => Domain.Common.Result.Success(),
        };

        await ExecuteDbContextAsync(async db =>
        {
            db.Tasks.Add(task);
            await db.SaveChangesAsync(CancellationToken);
        });
        return task;
    }

    /// <summary>Runs <paramref name="action"/> with a fresh <see cref="AppDbContext"/> (e.g. to assert stored state).</summary>
    protected async Task ExecuteDbContextAsync(Func<AppDbContext, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var scope = Factory.Services.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    protected async Task<T> ExecuteDbContextAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var scope = Factory.Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        var body = await response.Content.ReadFromJsonAsync<T>(Json, CancellationToken);
        return body ?? throw new InvalidOperationException($"Response body could not be read as {typeof(T).Name}.");
    }

    protected static Task<ProblemResponse> ReadProblemAsync(HttpResponseMessage response) =>
        ReadAsync<ProblemResponse>(response);

    protected Task<HttpResponseMessage> PostAsync<T>(string url, T body) =>
        Client.PostAsJsonAsync(url, body, Json, CancellationToken);

    protected Task<HttpResponseMessage> PutAsync<T>(string url, T body) =>
        Client.PutAsJsonAsync(url, body, Json, CancellationToken);
}
