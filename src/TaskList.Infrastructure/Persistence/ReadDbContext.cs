using Microsoft.EntityFrameworkCore;
using TaskList.Application.Abstractions.Data;
using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Infrastructure.Persistence;

/// <summary>Runs the LINQ queries composed by query handlers with EF Core, without change tracking.</summary>
internal sealed class ReadDbContext(AppDbContext db) : IReadDbContext
{
    public IQueryable<Category> Categories => db.Categories.AsNoTracking();

    public IQueryable<TaskItem> Tasks => db.Tasks.AsNoTracking();

    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        EntityFrameworkQueryableExtensions.ToListAsync(query, cancellationToken);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(query, cancellationToken);

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        EntityFrameworkQueryableExtensions.CountAsync(query, cancellationToken);
}
