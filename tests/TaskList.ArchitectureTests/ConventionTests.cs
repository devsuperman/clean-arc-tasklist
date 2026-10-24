using NetArchTest.Rules;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Domain.Common;

namespace TaskList.ArchitectureTests;

public sealed class ConventionTests
{
    [Fact]
    public void DomainEntities_ShouldBeSealedOrAbstract()
    {
        var failing = Types.InAssembly(Layers.Domain)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes()
            .Where(type => !type.IsSealed && !type.IsAbstract)
            .Select(type => type.FullName)
            .ToList();

        failing.ShouldBeEmpty();
    }

    [Fact]
    public void Handlers_ShouldBeSealed()
    {
        Type[] handlerContracts = [typeof(ICommandHandler<>), typeof(ICommandHandler<,>), typeof(IQueryHandler<,>)];

        var failing = Layers.Application.GetTypes()
            .Where(type => type.IsClass && type.GetInterfaces().Any(contract =>
                contract.IsGenericType && handlerContracts.Contains(contract.GetGenericTypeDefinition())))
            .Where(type => !type.IsSealed)
            .Select(type => type.FullName)
            .ToList();

        failing.ShouldBeEmpty();
    }
}
