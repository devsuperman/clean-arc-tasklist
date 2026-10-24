using TaskList.Domain.Common;

namespace TaskList.Domain.UnitTests.Common;

public sealed class ResultTests
{
    private static readonly Error SampleError = new("Sample.Error", "Something failed.", ErrorType.Conflict);

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_KeepsError()
    {
        var result = Result.Failure(SampleError);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SampleError);
    }

    [Fact]
    public void Failure_WithNullError_Throws() =>
        Should.Throw<ArgumentNullException>(() => Result.Failure(null!));

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result result = SampleError;

        result.Error.ShouldBe(SampleError);
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        Result.Success("text").Value.ShouldBe("text");
    }

    [Fact]
    public void GenericFailure_AccessingValue_ThrowsInvalidOperationException()
    {
        Result<int> result = SampleError;

        result.IsFailure.ShouldBeTrue();
        Should.Throw<InvalidOperationException>(() => result.Value);
        Result.Failure<string>(SampleError).Error.ShouldBe(SampleError);
    }

    [Fact]
    public void GenericFailure_WithNullError_Throws() =>
        Should.Throw<ArgumentNullException>(() => Result<int>.Failure(null!));

    [Fact]
    public void ValidationError_ForField_HasCodeTypeAndFieldMessages()
    {
        var error = ValidationError.ForField("name", "Name is required.");

        error.Code.ShouldBe("Validation.Failed");
        error.Type.ShouldBe(ErrorType.Validation);
        error.Errors["name"].ShouldBe(["Name is required."]);
    }
}
