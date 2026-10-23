using System.Reflection;

namespace TaskList.ArchitectureTests;

internal static class Layers
{
    public const string DomainNamespace = "TaskList.Domain";
    public const string ApplicationNamespace = "TaskList.Application";
    public const string InfrastructureNamespace = "TaskList.Infrastructure";
    public const string ApiNamespace = "TaskList.Api";

    public static readonly Assembly Domain = Assembly.Load(DomainNamespace);
    public static readonly Assembly Application = Assembly.Load(ApplicationNamespace);
    public static readonly Assembly Infrastructure = Assembly.Load(InfrastructureNamespace);
    public static readonly Assembly Api = Assembly.Load(ApiNamespace);
}
