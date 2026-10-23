using FluentValidation;

namespace TaskList.Application.Categories.Create;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Name).ValidCategoryName();
        RuleFor(command => command.Description).ValidCategoryDescription();
        RuleFor(command => command.Color).ValidCategoryColor();
    }
}
