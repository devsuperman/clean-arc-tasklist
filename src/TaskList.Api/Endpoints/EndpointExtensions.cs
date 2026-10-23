using System.Reflection;

namespace TaskList.Api.Endpoints;

public static class EndpointExtensions
{
    /// <summary>Maps every <see cref="IEndpoint"/> of the Api assembly into <paramref name="app"/>.</summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app) =>
        app.MapEndpoints(typeof(EndpointExtensions).Assembly);

    internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpoints = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => (IEndpoint)Activator.CreateInstance(type)!);

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
