using TaskList.Application.Categories.List;

namespace TaskList.Application.UnitTests.Categories.List;

public sealed class ListCategoriesQueryValidatorTests
{
    private readonly ListCategoriesQueryValidator _validator = new();

    [Theory] // BR-L01
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    public void Validate_WithPagingInRange_Passes(int page, int pageSize) =>
        _validator.Validate(new ListCategoriesQuery(page, pageSize)).IsValid.ShouldBeTrue();

    [Theory] // BR-L01
    [InlineData(0, 20, "Page")]
    [InlineData(1, 0, "PageSize")]
    [InlineData(1, 101, "PageSize")]
    public void Validate_WithPagingOutOfRange_Fails(int page, int pageSize, string property) =>
        _validator.Validate(new ListCategoriesQuery(page, pageSize))
            .Errors.Select(error => error.PropertyName).ShouldBe([property]);
}
