using TaskList.Application.Categories.Update;

namespace TaskList.Application.UnitTests.Categories.Update;

public sealed class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidValues_Passes() =>
        _validator.Validate(new UpdateCategoryCommand(Guid.NewGuid(), " Work ", null, "#1e90ff")).IsValid.ShouldBeTrue();

    [Fact] // BR-G01, BR-C03
    public void Validate_WithInvalidValues_FailsOnEachField() =>
        _validator.Validate(new UpdateCategoryCommand(Guid.NewGuid(), "  ", new string('a', 201), "#123"))
            .Errors.Select(error => error.PropertyName).ShouldBe(["Name", "Description", "Color"], ignoreOrder: true);
}
