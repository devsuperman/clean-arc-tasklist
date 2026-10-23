using Microsoft.EntityFrameworkCore;
using TaskList.Application.Abstractions.Data;
using TaskList.Domain.Tasks;

namespace TaskList.Infrastructure.Persistence.Repositories;

internal sealed class TaskRepository(AppDbContext db) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Tasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken) =>
        await db.Tasks.AddAsync(task, cancellationToken);

    public void Remove(TaskItem task) => db.Tasks.Remove(task);

    public Task<int> CountByCategoryAsync(Guid categoryId, CancellationToken cancellationToken) =>
        db.Tasks.CountAsync(task => task.CategoryId == categoryId, cancellationToken);
}
