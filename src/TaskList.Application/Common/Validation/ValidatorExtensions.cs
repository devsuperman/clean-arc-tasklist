using FluentValidation;
using TaskList.Domain.Common;

namespace TaskList.Application.Common.Validation;

internal static class ValidatorExtensions
{
    /// <summary>Runs every validator and returns a <see cref="ValidationError"/> with all field errors, or <c>null</c>.</summary>
    public static async Task<ValidationError?> ValidateAllAsync<T>(
        this IEnumerable<IValidator<T>> validators,
        T instance,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<T>(instance);
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return null;
        }

        var errors = failures
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        return new ValidationError(errors);
    }

    // "DueDate" -> "dueDate", so field names match the JSON contract.
    private static string ToCamelCase(string propertyName) =>
        string.IsNullOrEmpty(propertyName) || char.IsLower(propertyName[0])
            ? propertyName
            : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
