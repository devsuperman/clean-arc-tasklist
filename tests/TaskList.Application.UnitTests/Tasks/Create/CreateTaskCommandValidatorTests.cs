using TaskList.Application.Tasks.Create;
using TaskList.Domain.Tasks;

namespace TaskList.Application.UnitTests.Tasks.Create;

public sealed class CreateTaskCommandValidatorTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly CreateTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors() =>
        _validator.Validate(new CreateTaskCommand("Read book", "Chapter 1", CategoryId, TaskPriority.Low, null))
            .IsValid.ShouldBeTrue();

    [Theory] // BR-G01: lengths are checked on trimmed values
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ab  ")]
    public void Validate_TitleTooShort_FailsOnTitle(string? title) =>
        _validator.Validate(new CreateTaskCommand(title, null, CategoryId, null, null))
            .Errors.Select(error => error.PropertyName).ShouldBe(["Title"]);

    [Fact]
    public void Validate_TitleTooLong_FailsOnTitle() =>
        _validator.Validate(new CreateTaskCommand(new string('a', TaskItem.TitleMaxLength + 1), null, CategoryId, null, null))
            .Errors.Select(error => error.PropertyName).ShouldBe(["Title"]);

    [Fact]
    public void Validate_DescriptionTooLong_FailsOnDescription() =>
        _validator.Validate(new CreateTaskCommand("Read book", new string('a', TaskItem.DescriptionMaxLength + 1), CategoryId, null, null))
            .Errors.Select(error => error.PropertyName).ShouldBe(["Description"]);

    [Fact]
    public void Validate_EmptyCategoryIdAndUnknownPriority_FailsOnBoth() =>
        _validator.Validate(new CreateTaskCommand("Read book", null, Guid.Empty, (TaskPriority)42, null))
            .Errors.Select(error => error.PropertyName).ShouldBe(["CategoryId", "Priority"], ignoreOrder: true);
}
