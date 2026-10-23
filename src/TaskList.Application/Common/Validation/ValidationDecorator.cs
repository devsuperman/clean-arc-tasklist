using FluentValidation;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Common;

namespace TaskList.Application.Common.Validation;

/// <summary>
/// Wraps every handler: runs the FluentValidation validators of the request first and returns
/// <c>Validation.Failed</c> without calling the handler when any of them fails.
/// </summary>
internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> inner,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var error = await validators.ValidateAllAsync(command, cancellationToken);
            return error ?? await inner.Handle(command, cancellationToken);
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var error = await validators.ValidateAllAsync(command, cancellationToken);
            return error ?? await inner.Handle(command, cancellationToken);
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> inner,
        IEnumerable<IValidator<TQuery>> validators) : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var error = await validators.ValidateAllAsync(query, cancellationToken);
            return error ?? await inner.Handle(query, cancellationToken);
        }
    }
}
