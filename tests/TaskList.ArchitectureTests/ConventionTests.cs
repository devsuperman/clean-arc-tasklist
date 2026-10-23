using NetArchTest.Rules;
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
}
