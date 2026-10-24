using Microsoft.Extensions.DependencyInjection;

namespace TaskList.Api.IntegrationTests.Infrastructure;

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        foreach (var descriptor in services.Where(descriptor => descriptor.ServiceType == typeof(T)).ToList())
        {
            services.Remove(descriptor);
        }
    }
}
