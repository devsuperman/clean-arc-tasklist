using FluentValidation;
using TaskList.Application.Tasks.List;

namespace TaskList.Application.Tasks.ListByCategory;

internal sealed class ListCategoryTasksQueryValidator : AbstractValidator<ListCategoryTasksQuery>
{
    public ListCategoryTasksQueryValidator() => this.ApplyTaskListRules();
}
