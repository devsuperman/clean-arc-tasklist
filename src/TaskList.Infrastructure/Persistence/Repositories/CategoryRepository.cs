using Microsoft.EntityFrameworkCore;
using TaskList.Application.Abstractions.Data;
using TaskList.Domain.Categories;

namespace TaskList.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Categories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(category => category.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(name);
        var normalized = name.Trim().ToLowerInvariant();

#pragma warning disable CA1304, CA1311, CA1862 // ToLower() is translated to SQL lower(), matching the unique index.
        return db.Categories.AnyAsync(
            category => category.Name.ToLower() == normalized && (excludeId == null || category.Id != excludeId),
            cancellationToken);
#pragma warning restore CA1304, CA1311, CA1862
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken) =>
        await db.Categories.AddAsync(category, cancellationToken);

    public void Remove(Category category) => db.Categories.Remove(category);
}
