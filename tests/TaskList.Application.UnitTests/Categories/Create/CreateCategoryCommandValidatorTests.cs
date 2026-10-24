using TaskList.Application.Categories.Create;
using TaskList.Domain.Categories;

namespace TaskList.Application.UnitTests.Categories.Create;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Theory]
    [InlineData("Wo", null, null)]
    [InlineData("Work", "  ", "  ")] // BR-G01: blank optional fields are allowed
    [InlineData("  Work  ", "Office", "#1e90FF")]
    public void Validate_WithValidValues_Passes(string name, string? description, string? color) =>
        _validator.Validate(new CreateCategoryCommand(name, description, color)).IsValid.ShouldBeTrue();

    [Theory] // BR-G01: values are trimmed before checking length
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData(" W ")]
    public void Validate_WithInvalidName_FailsOnName(string? name) =>
        _validator.Validate(new CreateCategoryCommand(name, null, null))
            .Errors.Select(error => error.PropertyName).ShouldBe(["Name"]);

    [Fact]
    public void Validate_WithTooLongName_FailsOnName() =>
        _validator.Validate(new CreateCategoryCommand(new string('a', Category.NameMaxLength + 1), null, null))
            .IsValid.ShouldBeFalse();

    [Fact]
    public void Validate_WithTooLongDescription_FailsOnDescription() =>
        _validator.Validate(new CreateCategoryCommand("Work", new string('a', Category.DescriptionMaxLength + 1), null))
            .Errors.Select(error => error.PropertyName).ShouldBe(["Description"]);

    [Theory] // BR-C03
    [InlineData("red")]
    [InlineData("#12345")]
    [InlineData("#GGGGGG")]
    public void Validate_WithInvalidColor_FailsOnColor(string color) =>
        _validator.Validate(new CreateCategoryCommand("Work", null, color))
            .Errors.Select(error => error.PropertyName).ShouldBe(["Color"]);
}
