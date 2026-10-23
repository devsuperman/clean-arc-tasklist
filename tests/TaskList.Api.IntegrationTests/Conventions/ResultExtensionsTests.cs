using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaskList.Api.Infrastructure;
using TaskList.Domain.Common;

namespace TaskList.Api.IntegrationTests.Conventions;

public sealed class ResultExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unexpected, StatusCodes.Status500InternalServerError)]
    public void ToProblem_MapsErrorTypeToStatusCode(ErrorType type, int expectedStatus)
    {
        var result = Result.Failure(new Error("Sample.Code", "Sample message.", type));

        var problem = result.ToProblem().ShouldBeOfType<ProblemHttpResult>();

        problem.StatusCode.ShouldBe(expectedStatus);
        problem.ProblemDetails.Detail.ShouldBe("Sample message.");
        problem.ProblemDetails.Extensions["code"].ShouldBe("Sample.Code");
    }

    [Fact]
    public void ToProblem_WithValidationError_ReturnsFieldErrors()
    {
        var result = Result.Failure(ValidationError.ForField("title", "Title is required."));

        var problem = result.ToProblem().ShouldBeOfType<ProblemHttpResult>();

        problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Extensions["code"].ShouldBe("Validation.Failed");
        var details = problem.ProblemDetails.ShouldBeOfType<HttpValidationProblemDetails>();
        details.Errors["title"].ShouldBe(["Title is required."]);
    }

    [Fact]
    public void ToProblem_WithSuccess_Throws() =>
        Should.Throw<InvalidOperationException>(() => Result.Success().ToProblem());
}
