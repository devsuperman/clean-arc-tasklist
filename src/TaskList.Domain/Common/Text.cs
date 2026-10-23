namespace TaskList.Domain.Common;

internal static class Text
{
    /// <summary>BR-G01: inputs are trimmed; whitespace-only becomes empty.</summary>
    public static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    /// <summary>BR-G01 for optional fields: trimmed, and empty becomes <c>null</c>.</summary>
    public static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
