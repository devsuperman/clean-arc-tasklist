using System.Text.RegularExpressions;
using TaskList.Domain.Common;

namespace TaskList.Domain.Categories;

public sealed partial class Category : Entity
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;
    public const int DescriptionMaxLength = 200;
    public const int ColorLength = 7;

    private Category(Guid id, DateTimeOffset createdAt)
        : base(id) => CreatedAt = createdAt;

    // Required by EF Core.
    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    /// <summary>Hex color <c>#RRGGBB</c>, stored uppercase (BR-C03).</summary>
    public string? Color { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Result<Category> Create(string? name, string? description, string? color, DateTimeOffset now)
    {
        var category = new Category(Guid.CreateVersion7(now), now);
        var result = category.Apply(name, description, color);
        return result.IsSuccess ? category : result.Error;
    }

    public Result Update(string? name, string? description, string? color, DateTimeOffset now)
    {
        var result = Apply(name, description, color);
        if (result.IsSuccess)
        {
            UpdatedAt = now;
        }

        return result;
    }

    /// <summary>Case-insensitive name comparison used by uniqueness rules (BR-C01, BR-C02).</summary>
    public bool HasName(string? name) =>
        string.Equals(Name, Text.Normalize(name), StringComparison.OrdinalIgnoreCase);

    private Result Apply(string? name, string? description, string? color)
    {
        var normalizedName = Text.Normalize(name);
        if (normalizedName.Length is < NameMinLength or > NameMaxLength)
        {
            return CategoryErrors.NameInvalidLength;
        }

        var normalizedDescription = Text.NormalizeOptional(description);
        if (normalizedDescription?.Length > DescriptionMaxLength)
        {
            return CategoryErrors.DescriptionTooLong;
        }

        var normalizedColor = Text.NormalizeOptional(color);
        if (normalizedColor is not null && !ColorRegex().IsMatch(normalizedColor))
        {
            return CategoryErrors.ColorInvalid;
        }

        Name = normalizedName;
        Description = normalizedDescription;
        Color = normalizedColor?.ToUpperInvariant();
        return Result.Success();
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex ColorRegex();
}
