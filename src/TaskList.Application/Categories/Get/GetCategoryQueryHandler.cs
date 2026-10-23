using TaskList.Application.Abstractions.Data;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Categories;
using TaskList.Domain.Common;

namespace TaskList.Application.Categories.Get;

internal sealed class GetCategoryQueryHandler(IReadDbContext db) : IQueryHandler<GetCategoryQuery, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        var category = await db.FirstOrDefaultAsync(
            db.Categories.Where(c => c.Id == query.Id).SelectResponse(db.Tasks),
            cancellationToken);

        return category is null ? CategoryErrors.NotFound(query.Id) : category;
    }
}
