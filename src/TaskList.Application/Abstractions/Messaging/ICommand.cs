namespace TaskList.Application.Abstractions.Messaging;

/// <summary>A use case that changes state and returns no value.</summary>
#pragma warning disable CA1040 // Marker interface: used as a generic constraint and by assembly scanning.
public interface ICommand;
#pragma warning restore CA1040

/// <summary>A use case that changes state and returns <typeparamref name="TResponse"/>.</summary>
#pragma warning disable CA1040
public interface ICommand<TResponse>;
#pragma warning restore CA1040
