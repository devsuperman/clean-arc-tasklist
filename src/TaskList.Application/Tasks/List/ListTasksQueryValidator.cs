using FluentValidation;

namespace TaskList.Application.Tasks.List;

internal sealed class ListTasksQueryValidator : AbstractValidator<ListTasksQuery>
{
    public ListTasksQueryValidator() => this.ApplyTaskListRules();
}
