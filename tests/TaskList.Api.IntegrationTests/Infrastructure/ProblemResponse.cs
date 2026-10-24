namespace TaskList.Api.IntegrationTests.Infrastructure;

/// <summary>RFC 9457 Problem Details as returned by the API.</summary>
public sealed record ProblemResponse(
    string? Type,
    string? Title,
    int Status,
    string? Detail,
    string? Code,
    string? TraceId,
    Dictionary<string, string[]>? Errors);
