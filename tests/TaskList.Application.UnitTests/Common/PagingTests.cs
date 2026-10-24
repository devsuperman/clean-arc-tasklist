using FluentValidation;
using TaskList.Application.Common.Paging;

namespace TaskList.Application.UnitTests.Common;

public sealed class PagingTests
{
    private sealed record SampleQuery(int Page, int PageSize) : IPagedQuery;

    private sealed class SampleValidator : AbstractValidator<SampleQuery>
    {
        public SampleValidator() => this.ApplyPagingRules();
    }

    [Theory] // BR-L01
    [InlineData(1, 1, true)]
    [InlineData(1, 20, true)]
    [InlineData(5, 100, true)]
    [InlineData(0, 20, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    public void PagingRules_ValidateRanges(int page, int pageSize, bool expected) =>
        new SampleValidator().Validate(new SampleQuery(page, pageSize)).IsValid.ShouldBe(expected);

    [Theory] // BR-L02
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    public void PagedResult_CalculatesTotalPages(int totalCount, int pageSize, int expected) =>
        new PagedResult<int>([], 1, pageSize, totalCount).TotalPages.ShouldBe(expected);
}
