using FluentValidation;

namespace TaskList.Application.Categories.Update;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Name).ValidCategoryName();
        RuleFor(command => command.Description).ValidCategoryDescription();
        RuleFor(command => command.Color).ValidCategoryColor();
    }
}
