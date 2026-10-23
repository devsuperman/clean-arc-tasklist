using FluentValidation;
using TaskList.Domain.Tasks;

namespace TaskList.Application.Tasks.Update;

/// <summary>
/// Input checks that mirror the domain limits (on trimmed values, BR-G01). The "due date in the past" rule
/// (BR-T04) is left to the domain, which needs the clock.
/// </summary>
internal sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(command => command.Title)
            .Must(title => title?.Trim().Length is >= TaskItem.TitleMinLength and <= TaskItem.TitleMaxLength)
            .WithMessage($"Title must have between {TaskItem.TitleMinLength} and {TaskItem.TitleMaxLength} characters.");

        RuleFor(command => command.Description)
            .Must(description => description is null || description.Trim().Length <= TaskItem.DescriptionMaxLength)
            .WithMessage($"Description must have at most {TaskItem.DescriptionMaxLength} characters.");

        RuleFor(command => command.CategoryId)
            .NotEmpty()
            .WithMessage("Category id is required.");

        RuleFor(command => command.Priority)
            .IsInEnum()
            .When(command => command.Priority is not null)
            .WithMessage("Priority must be Low, Medium or High.");
    }
}
