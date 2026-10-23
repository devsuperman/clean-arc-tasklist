using System.Diagnostics;

namespace TaskList.Api.Infrastructure;

internal static class ProblemDetailsExtensions
{
    /// <summary>Every Problem Details response carries a <c>traceId</c> so it can be found in the logs.</summary>
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions["traceId"] =
                Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
