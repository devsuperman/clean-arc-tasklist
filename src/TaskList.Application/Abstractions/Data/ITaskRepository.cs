using TaskList.Domain.Tasks;

namespace TaskList.Application.Abstractions.Data;

/// <summary>Write-side access to tasks. Changes are saved by <see cref="IUnitOfWork"/>.</summary>
public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    void Remove(TaskItem task);

    Task<int> CountByCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
}
