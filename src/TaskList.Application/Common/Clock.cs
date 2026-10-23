namespace TaskList.Application.Common;

public static class Clock
{
    /// <summary>Today's date in UTC (BR-G03, BR-G04). Use it for due dates and <c>IsOverdue</c>.</summary>
    public static DateOnly Today(this TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        return DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    }
}
