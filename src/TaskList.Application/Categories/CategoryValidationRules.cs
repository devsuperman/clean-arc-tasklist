using System.Text.RegularExpressions;
using FluentValidation;
using TaskList.Domain.Categories;

namespace TaskList.Application.Categories;

/// <summary>
/// Field rules shared by the create and update validators. They mirror the <see cref="Category"/> invariants
/// and check <b>trimmed</b> values (BR-G01).
/// </summary>
internal static partial class CategoryValidationRules
{
    public static IRuleBuilderOptions<T, string?> ValidCategoryName<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(name => name?.Trim().Length is >= Category.NameMinLength and <= Category.NameMaxLength)
            .WithMessage($"Name must have between {Category.NameMinLength} and {Category.NameMaxLength} characters.");

    public static IRuleBuilderOptions<T, string?> ValidCategoryDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(description => (description?.Trim().Length ?? 0) <= Category.DescriptionMaxLength)
            .WithMessage($"Description must have at most {Category.DescriptionMaxLength} characters.");

    // BR-C03: blank means "no color".
    public static IRuleBuilderOptions<T, string?> ValidCategoryColor<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(color => string.IsNullOrWhiteSpace(color) || ColorRegex().IsMatch(color.Trim()))
            .WithMessage("Color must be a hex color in the format #RRGGBB.");

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex ColorRegex();
}
