using TaskList.Domain.Categories;

namespace TaskList.Application.Abstractions.Data;

/// <summary>Write-side access to categories. Changes are saved by <see cref="IUnitOfWork"/>.</summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Whether another category already uses <paramref name="name"/>, ignoring case (BR-C01).
    /// Pass <paramref name="excludeId"/> when renaming, so a category does not collide with itself (BR-C02).
    /// </summary>
    Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken cancellationToken);

    Task AddAsync(Category category, CancellationToken cancellationToken);

    void Remove(Category category);
}
