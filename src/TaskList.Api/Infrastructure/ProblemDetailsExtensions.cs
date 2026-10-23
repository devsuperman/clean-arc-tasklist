using System.Diagnostics;
using TaskList.Domain.Common;

namespace TaskList.Api.Infrastructure;

internal static class ProblemDetailsExtensions
{
    /// <summary>
    /// Every Problem Details response carries a <c>traceId</c>. It is the W3C trace id that Serilog also writes on
    /// every log event of the request (<c>TraceId</c> / <c>@tr</c>), so a failing request can be found in the logs.
    /// </summary>
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions["traceId"] =
                Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;

            // Requests rejected by the framework itself (malformed JSON, an unknown enum name in the query string)
            // never reach a handler; they are still validation failures for the client.
            if (context.ProblemDetails.Status == StatusCodes.Status400BadRequest)
            {
                context.ProblemDetails.Extensions.TryAdd("code", ValidationError.ErrorCode);
            }
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
