namespace TaskList.Application.Abstractions.Messaging;

/// <summary>A use case that only reads data.</summary>
#pragma warning disable CA1040 // Marker interface: used as a generic constraint and by assembly scanning.
public interface IQuery<TResponse>;
#pragma warning restore CA1040
