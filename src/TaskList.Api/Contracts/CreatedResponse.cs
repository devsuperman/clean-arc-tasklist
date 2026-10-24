namespace TaskList.Api.Contracts;

/// <summary>Body of every <c>201 Created</c> response; the full resource is at the <c>Location</c> header.</summary>
public sealed record CreatedResponse(Guid Id);
