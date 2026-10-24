using NetArchTest.Rules;

namespace TaskList.ArchitectureTests;

public sealed class DependencyRuleTests
{
    [Fact]
    public void Domain_ShouldNotDependOn_AnyOtherLayerOrFramework()
    {
        var result = Types.InAssembly(Layers.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                Layers.ApplicationNamespace,
                Layers.InfrastructureNamespace,
                Layers.ApiNamespace,
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "FluentValidation")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Application_ShouldNotDependOn_InfrastructureApiOrFrameworks()
    {
        var result = Types.InAssembly(Layers.Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                Layers.InfrastructureNamespace,
                Layers.ApiNamespace,
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Api()
    {
        var result = Types.InAssembly(Layers.Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(Layers.ApiNamespace)
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }

    [Fact]
    public void ApiEndpoints_ShouldNotDependOn_Infrastructure()
    {
        // Only the composition root (Program / DI wiring) may reference Infrastructure.
        var result = Types.InAssembly(Layers.Api)
            .That()
            .ResideInNamespaceStartingWith($"{Layers.ApiNamespace}.Endpoints")
            .Or()
            .ResideInNamespaceStartingWith($"{Layers.ApiNamespace}.Contracts")
            .ShouldNot()
            .HaveDependencyOn(Layers.InfrastructureNamespace)
            .GetResult();

        result.FailingTypeNames.ShouldBeNull();
    }
}
