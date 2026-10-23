namespace TaskList.Api.IntegrationTests.Infrastructure;

/// <summary>A clock tests can set freely (forwards and backwards), replacing <see cref="TimeProvider.System"/>.</summary>
public sealed class TestTimeProvider : TimeProvider
{
    public static readonly DateTimeOffset DefaultNow = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    public DateTimeOffset UtcNow { get; set; } = DefaultNow;

    public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);

    public override DateTimeOffset GetUtcNow() => UtcNow;

    public void Advance(TimeSpan delta) => UtcNow += delta;
}
