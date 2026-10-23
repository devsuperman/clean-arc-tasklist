using TaskList.Domain.Categories;
using TaskList.Domain.Common;

namespace TaskList.Domain.UnitTests.Categories;

public sealed class CategoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_SetsFieldsAndCreatedAt()
    {
        var category = Category.Create("Work", "Office stuff", "#1e90ff", Now).Value;

        category.Id.ShouldNotBe(Guid.Empty);
        category.Name.ShouldBe("Work");
        category.Description.ShouldBe("Office stuff");
        category.CreatedAt.ShouldBe(Now);
        category.UpdatedAt.ShouldBeNull();
    }

    [Fact] // BR-G01
    public void Create_TrimsInputs_AndTurnsBlankOptionalFieldsIntoNull()
    {
        var category = Category.Create("  Home  ", "   ", "  ", Now).Value;

        category.Name.ShouldBe("Home");
        category.Description.ShouldBeNull();
        category.Color.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData(" A ")]
    public void Create_WithTooShortName_FailsValidation(string? name)
    {
        var result = Category.Create(name, null, null, Now);

        result.Error.ShouldBe(CategoryErrors.NameInvalidLength);
    }

    [Fact]
    public void Create_NameLengthBoundaries()
    {
        Category.Create("ab", null, null, Now).IsSuccess.ShouldBeTrue();
        Category.Create(new string('a', 50), null, null, Now).IsSuccess.ShouldBeTrue();
        Category.Create(new string('a', 51), null, null, Now).Error.ShouldBe(CategoryErrors.NameInvalidLength);
    }

    [Fact]
    public void Create_DescriptionLengthBoundaries()
    {
        Category.Create("Work", new string('d', 200), null, Now).IsSuccess.ShouldBeTrue();
        Category.Create("Work", new string('d', 201), null, Now).Error.ShouldBe(CategoryErrors.DescriptionTooLong);
    }

    [Theory] // BR-C03
    [InlineData("#1e90ff", "#1E90FF")]
    [InlineData("#ABCDEF", "#ABCDEF")]
    [InlineData(" #00aa11 ", "#00AA11")]
    public void Create_WithValidColor_NormalizesToUppercase(string color, string expected) =>
        Category.Create("Work", null, color, Now).Value.Color.ShouldBe(expected);

    [Theory] // BR-C03
    [InlineData("1E90FF")]
    [InlineData("#1E90F")]
    [InlineData("#1E90FFF")]
    [InlineData("#GGGGGG")]
    [InlineData("blue")]
    public void Create_WithInvalidColor_FailsValidation(string color) =>
        Category.Create("Work", null, color, Now).Error.ShouldBe(CategoryErrors.ColorInvalid);

    [Fact]
    public void Update_WithValidData_ChangesFieldsAndSetsUpdatedAt()
    {
        var category = Category.Create("Work", null, null, Now).Value;
        var later = Now.AddHours(1);

        var result = category.Update(" Job ", "New", "#abcdef", later);

        result.IsSuccess.ShouldBeTrue();
        category.Name.ShouldBe("Job");
        category.Description.ShouldBe("New");
        category.Color.ShouldBe("#ABCDEF");
        category.UpdatedAt.ShouldBe(later);
        category.CreatedAt.ShouldBe(Now);
    }

    [Fact]
    public void Update_WithInvalidData_KeepsPreviousState()
    {
        var category = Category.Create("Work", "Desc", "#000000", Now).Value;

        var result = category.Update("Job", null, "nope", Now.AddHours(1));

        result.Error.ShouldBe(CategoryErrors.ColorInvalid);
        category.Name.ShouldBe("Work");
        category.Description.ShouldBe("Desc");
        category.Color.ShouldBe("#000000");
        category.UpdatedAt.ShouldBeNull();
    }

    [Theory] // BR-C01, BR-C02
    [InlineData("Work", true)]
    [InlineData("work", true)]
    [InlineData(" WORK ", true)]
    [InlineData("Home", false)]
    public void HasName_ComparesIgnoringCaseAndSpaces(string name, bool expected) =>
        Category.Create("Work", null, null, Now).Value.HasName(name).ShouldBe(expected);

    [Fact]
    public void Errors_FollowTheErrorCatalog()
    {
        var id = Guid.NewGuid();

        CategoryErrors.NotFound(id).Code.ShouldBe("Category.NotFound");
        CategoryErrors.NotFound(id).Type.ShouldBe(ErrorType.NotFound);
        CategoryErrors.NameAlreadyExists("Work").Code.ShouldBe("Category.NameAlreadyExists");
        CategoryErrors.NameAlreadyExists("Work").Type.ShouldBe(ErrorType.Conflict);
        CategoryErrors.HasTasks(3).Code.ShouldBe("Category.HasTasks");
        CategoryErrors.HasTasks(3).Message.ShouldContain("3");
        CategoryErrors.HasTasks(3).Type.ShouldBe(ErrorType.Conflict);
    }
}
