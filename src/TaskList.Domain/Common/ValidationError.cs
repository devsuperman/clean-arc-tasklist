namespace TaskList.Domain.Common;

/// <summary>One or more field validations failed. <see cref="Errors"/> maps each field to its messages.</summary>
public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors)
    : Error(ValidationError.ErrorCode, "One or more validation errors occurred.", ErrorType.Validation)
{
    public const string ErrorCode = "Validation.Failed";

    public static ValidationError ForField(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });
}
