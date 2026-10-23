using FluentValidation;
using TaskList.Application.Common.Paging;

namespace TaskList.Application.Categories.List;

internal sealed class ListCategoriesQueryValidator : AbstractValidator<ListCategoriesQuery>
{
    public ListCategoriesQueryValidator() => this.ApplyPagingRules();
}
