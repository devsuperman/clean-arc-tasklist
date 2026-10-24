using TaskList.Domain.Common;

namespace TaskList.Domain.Categories;

public static class CategoryErrors
{
    public static Error NotFound(Guid id) =>
        new("Category.NotFound", $"Category '{id}' was not found.", ErrorType.NotFound);

    public static Error NameAlreadyExists(string name) =>
        new("Category.NameAlreadyExists", $"A category named '{name}' already exists.", ErrorType.Conflict);

    public static Error HasTasks(int taskCount) =>
        new(
            "Category.HasTasks",
            $"The category has {taskCount} task(s). Move or delete them before deleting the category.",
            ErrorType.Conflict);

    public static readonly ValidationError NameInvalidLength = ValidationError.ForField(
        "name",
        $"Name must have between {Category.NameMinLength} and {Category.NameMaxLength} characters.");

    public static readonly ValidationError DescriptionTooLong = ValidationError.ForField(
        "description",
        $"Description must have at most {Category.DescriptionMaxLength} characters.");

    public static readonly ValidationError ColorInvalid = ValidationError.ForField(
        "color",
        "Color must be a hex color in the format #RRGGBB.");
}
