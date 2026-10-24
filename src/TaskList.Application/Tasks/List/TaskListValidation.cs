using FluentValidation;
using TaskList.Application.Common.Paging;

namespace TaskList.Application.Tasks.List;

public static class TaskListValidation
{
    /// <summary>Adds the rules shared by every task listing: paging (BR-L01), due range (BR-L04) and sorting (BR-L05).</summary>
    public static void ApplyTaskListRules<T>(this AbstractValidator<T> validator)
        where T : ITaskListQuery
    {
        ArgumentNullException.ThrowIfNull(validator);
        validator.ApplyPagingRules();

        validator.RuleFor(query => query.Status).IsInEnum();
        validator.RuleFor(query => query.Priority).IsInEnum();

        validator.RuleFor(query => query.DueTo)
            .GreaterThanOrEqualTo(query => query.DueFrom)
            .When(query => query.DueFrom is not null && query.DueTo is not null)
            .WithMessage("'dueTo' must be on or after 'dueFrom'.");

        validator.RuleFor(query => query.SortBy)
            .Must(sortBy => TaskSort.Fields.Any(field => TaskSort.IsField(sortBy, field)))
            .When(query => query.SortBy is not null)
            .WithMessage($"'sortBy' must be one of: {string.Join(", ", TaskSort.Fields)}.");

        validator.RuleFor(query => query.SortDirection)
            .Must(direction => TaskSort.Directions.Any(value => TaskSort.IsField(direction, value)))
            .When(query => query.SortDirection is not null)
            .WithMessage($"'sortDirection' must be one of: {string.Join(", ", TaskSort.Directions)}.");
    }
}
