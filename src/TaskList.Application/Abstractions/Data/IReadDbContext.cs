using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Abstractions.Data;

/// <summary>
/// Read side used by query handlers. Queries are composed with plain LINQ (<see cref="Queryable"/>), so the
/// Application layer does not depend on EF Core; Infrastructure runs them without change tracking.
/// Unit tests can back it with in-memory collections.
/// </summary>
public interface IReadDbContext
{
    IQueryable<Category> Categories { get; }

    IQueryable<TaskItem> Tasks { get; }

    Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken);

    Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken);
}
