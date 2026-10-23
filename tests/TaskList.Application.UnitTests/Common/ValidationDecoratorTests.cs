using FluentValidation;
using NSubstitute;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Validation;
using TaskList.Domain.Common;

namespace TaskList.Application.UnitTests.Common;

public sealed class ValidationDecoratorTests
{
    public sealed record SampleCommand(string Title, int DueDate) : ICommand, ICommand<Guid>, IQuery<Guid>;

    private sealed class SampleValidator : AbstractValidator<SampleCommand>
    {
        public SampleValidator()
        {
            RuleFor(command => command.Title).NotEmpty();
            RuleFor(command => command.DueDate).GreaterThan(0);
        }
    }

    private static readonly SampleCommand InvalidCommand = new("", 0);
    private static readonly SampleCommand ValidCommand = new("Title", 1);

    [Fact]
    public async Task Command_WhenInvalid_NeverReachesHandler()
    {
        var inner = Substitute.For<ICommandHandler<SampleCommand>>();
        var decorator = new ValidationDecorator.CommandHandler<SampleCommand>(inner, [new SampleValidator()]);

        var result = await decorator.Handle(InvalidCommand, TestContext.Current.CancellationToken);

        await inner.DidNotReceive().Handle(Arg.Any<SampleCommand>(), Arg.Any<CancellationToken>());
        var error = result.Error.ShouldBeOfType<ValidationError>();
        error.Code.ShouldBe("Validation.Failed");
        error.Errors.Keys.ShouldBe(["title", "dueDate"], ignoreOrder: true);
    }

    [Fact]
    public async Task Command_WhenValid_CallsHandler()
    {
        var inner = Substitute.For<ICommandHandler<SampleCommand>>();
        inner.Handle(ValidCommand, Arg.Any<CancellationToken>()).Returns(Result.Success());
        var decorator = new ValidationDecorator.CommandHandler<SampleCommand>(inner, [new SampleValidator()]);

        var result = await decorator.Handle(ValidCommand, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await inner.Received(1).Handle(ValidCommand, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CommandWithResponse_WhenInvalid_NeverReachesHandler()
    {
        var inner = Substitute.For<ICommandHandler<SampleCommand, Guid>>();
        var decorator = new ValidationDecorator.CommandHandler<SampleCommand, Guid>(inner, [new SampleValidator()]);

        var result = await decorator.Handle(InvalidCommand, TestContext.Current.CancellationToken);

        await inner.DidNotReceive().Handle(Arg.Any<SampleCommand>(), Arg.Any<CancellationToken>());
        result.Error.ShouldBeOfType<ValidationError>();
    }

    [Fact]
    public async Task CommandWithResponse_WhenValid_ReturnsHandlerValue()
    {
        var id = Guid.NewGuid();
        var inner = Substitute.For<ICommandHandler<SampleCommand, Guid>>();
        inner.Handle(ValidCommand, Arg.Any<CancellationToken>()).Returns(Result.Success(id));
        var decorator = new ValidationDecorator.CommandHandler<SampleCommand, Guid>(inner, [new SampleValidator()]);

        (await decorator.Handle(ValidCommand, TestContext.Current.CancellationToken)).Value.ShouldBe(id);
    }

    [Fact]
    public async Task Query_WhenInvalid_NeverReachesHandler()
    {
        var inner = Substitute.For<IQueryHandler<SampleCommand, Guid>>();
        var decorator = new ValidationDecorator.QueryHandler<SampleCommand, Guid>(inner, [new SampleValidator()]);

        var result = await decorator.Handle(InvalidCommand, TestContext.Current.CancellationToken);

        await inner.DidNotReceive().Handle(Arg.Any<SampleCommand>(), Arg.Any<CancellationToken>());
        result.Error.ShouldBeOfType<ValidationError>();
    }

    [Fact]
    public async Task Query_WithoutValidators_CallsHandler()
    {
        var inner = Substitute.For<IQueryHandler<SampleCommand, Guid>>();
        inner.Handle(InvalidCommand, Arg.Any<CancellationToken>()).Returns(Result.Success(Guid.Empty));
        var decorator = new ValidationDecorator.QueryHandler<SampleCommand, Guid>(inner, []);

        (await decorator.Handle(InvalidCommand, TestContext.Current.CancellationToken)).IsSuccess.ShouldBeTrue();
    }
}
