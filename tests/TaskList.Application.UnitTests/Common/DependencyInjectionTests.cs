using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Common;

namespace TaskList.Application.UnitTests.Common;

public sealed class DependencyInjectionTests
{
    public sealed record PingCommand(string Message) : ICommand<string>;

    public sealed class PingValidator : AbstractValidator<PingCommand>
    {
        public PingValidator() => RuleFor(command => command.Message).NotEmpty();
    }

    public sealed class PingHandler : ICommandHandler<PingCommand, string>
    {
        public Task<Result<string>> Handle(PingCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success($"pong: {command.Message}"));
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddValidatorsFromAssemblyContaining<PingValidator>(includeInternalTypes: true);
        services.AddHandlersFromAssembly(typeof(PingHandler).Assembly);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task AddHandlersFromAssembly_ResolvesHandlerWrappedByValidation()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<PingCommand, string>>();

        (await handler.Handle(new PingCommand("hi"), TestContext.Current.CancellationToken)).Value.ShouldBe("pong: hi");
        (await handler.Handle(new PingCommand(""), TestContext.Current.CancellationToken)).Error.ShouldBeOfType<ValidationError>();
    }

    [Fact]
    public void AddApplication_RegistersWithoutErrors() =>
        Should.NotThrow(() => new ServiceCollection().AddApplication().BuildServiceProvider().Dispose());
}
