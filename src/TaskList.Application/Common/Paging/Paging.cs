using FluentValidation;

namespace TaskList.Application.Common.Paging;

/// <summary>Pagination limits from BR-L01.</summary>
public static class Paging
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Adds the BR-L01 rules: <c>page &gt;= 1</c> and <c>1 &lt;= pageSize &lt;= 100</c>.</summary>
    public static void ApplyPagingRules<T>(this AbstractValidator<T> validator)
        where T : IPagedQuery
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        validator.RuleFor(query => query.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
