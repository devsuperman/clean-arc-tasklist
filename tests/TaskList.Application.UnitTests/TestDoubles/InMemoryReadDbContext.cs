using TaskList.Application.Abstractions.Data;
using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.TestDoubles;

/// <summary>An <see cref="IReadDbContext"/> backed by lists, for unit testing query handlers.</summary>
public sealed class InMemoryReadDbContext : IReadDbContext
{
    public List<Category> CategoryList { get; } = [];

    public List<TaskItem> TaskList { get; } = [];

    public IQueryable<Category> Categories => CategoryList.AsQueryable();

    public IQueryable<TaskItem> Tasks => TaskList.AsQueryable();

    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        Task.FromResult(query.ToList());

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        Task.FromResult(query.FirstOrDefault());

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        Task.FromResult(query.Count());
}
