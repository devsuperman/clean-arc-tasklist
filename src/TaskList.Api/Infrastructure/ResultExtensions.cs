using TaskList.Domain.Common;

namespace TaskList.Api.Infrastructure;

public static class ResultExtensions
{
    /// <summary>
    /// Turns a failed <see cref="Result"/> into RFC 9457 Problem Details with the error <c>code</c>.
    /// <c>Validation → 400</c>, <c>NotFound → 404</c>, <c>Conflict → 409</c>, <c>Unexpected → 500</c>.
    /// </summary>
    public static IResult ToProblem(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be converted to a problem.");
        }

        var error = result.Error;
        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };

        if (error is ValidationError validationError)
        {
            return Results.ValidationProblem(
                validationError.Errors.ToDictionary(),
                detail: error.Message,
                title: "Validation failed",
                extensions: extensions);
        }

        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad request"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Server failure"),
        };

        return Results.Problem(statusCode: statusCode, title: title, detail: error.Message, extensions: extensions);
    }
}
