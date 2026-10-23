using TaskList.Application.Tasks.List;

namespace TaskList.Application.UnitTests.Tasks.List;

public sealed class ListTasksQueryValidatorTests
{
    private static readonly DateOnly Day = new(2026, 10, 23);
    private readonly ListTasksQueryValidator _validator = new();

    [Fact]
    public void Validate_Defaults_IsValid() =>
        _validator.Validate(new ListTasksQuery()).IsValid.ShouldBeTrue();

    [Theory] // BR-L01
    [InlineData(0, 20, "Page")]
    [InlineData(1, 0, "PageSize")]
    [InlineData(1, 101, "PageSize")]
    public void Validate_PagingOutOfRange_Fails(int page, int pageSize, string property)
    {
        var result = _validator.Validate(new ListTasksQuery(Page: page, PageSize: pageSize));

        result.Errors.Select(error => error.PropertyName).ShouldBe([property]);
    }

    [Fact] // BR-L04
    public void Validate_DueFromAfterDueTo_Fails()
    {
        var result = _validator.Validate(new ListTasksQuery(DueFrom: Day.AddDays(1), DueTo: Day));

        result.Errors.Select(error => error.PropertyName).ShouldBe(["DueTo"]);
    }

    [Fact] // BR-L04
    public void Validate_DueFromEqualToDueTo_IsValid() =>
        _validator.Validate(new ListTasksQuery(DueFrom: Day, DueTo: Day)).IsValid.ShouldBeTrue();

    [Theory] // BR-L05
    [InlineData("createdAt", "asc")]
    [InlineData("DueDate", "DESC")]
    [InlineData("priority", null)]
    [InlineData(null, "asc")]
    [InlineData("title", "desc")]
    public void Validate_KnownSort_IsValid(string? sortBy, string? direction) =>
        _validator.Validate(new ListTasksQuery(SortBy: sortBy, SortDirection: direction)).IsValid.ShouldBeTrue();

    [Theory] // BR-L05
    [InlineData("status", null, "SortBy")]
    [InlineData("", null, "SortBy")]
    [InlineData(null, "up", "SortDirection")]
    public void Validate_UnknownSort_Fails(string? sortBy, string? direction, string property)
    {
        var result = _validator.Validate(new ListTasksQuery(SortBy: sortBy, SortDirection: direction));

        result.Errors.Select(error => error.PropertyName).ShouldBe([property]);
    }

    [Fact]
    public void Validate_UndefinedEnumValue_Fails()
    {
        var result = _validator.Validate(new ListTasksQuery(Status: (TaskStatus)42));

        result.Errors.Select(error => error.PropertyName).ShouldBe(["Status"]);
    }
}
