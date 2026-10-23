using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskList.Application.Abstractions.Messaging;
using TaskList.Application.Common.Validation;

namespace TaskList.Application;

public static class DependencyInjection
{
    private static readonly Dictionary<Type, Type> Decorators = new()
    {
        [typeof(ICommandHandler<>)] = typeof(ValidationDecorator.CommandHandler<>),
        [typeof(ICommandHandler<,>)] = typeof(ValidationDecorator.CommandHandler<,>),
        [typeof(IQueryHandler<,>)] = typeof(ValidationDecorator.QueryHandler<,>),
    };

    /// <summary>Registers every validator and handler of this assembly. New use cases need no changes here.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddHandlersFromAssembly(assembly);
        return services;
    }

    /// <summary>
    /// Registers each handler as its concrete type, and its handler interface as the same handler
    /// wrapped by the validation decorator.
    /// </summary>
    internal static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlers = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.IsGenericType && Decorators.ContainsKey(contract.GetGenericTypeDefinition()))
                .Select(contract => (Implementation: type, Contract: contract)));

        foreach (var (implementation, contract) in handlers)
        {
            var decorator = Decorators[contract.GetGenericTypeDefinition()].MakeGenericType(contract.GetGenericArguments());

            services.AddScoped(implementation);
            services.AddScoped(contract, provider =>
                ActivatorUtilities.CreateInstance(provider, decorator, provider.GetRequiredService(implementation)));
        }

        return services;
    }
}
