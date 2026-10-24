using TaskList.Application.Categories.List;
using TaskList.Application.UnitTests.TestDoubles;

namespace TaskList.Application.UnitTests.Categories.List;

public sealed class ListCategoriesQueryHandlerTests
{
    private readonly InMemoryReadDbContext _db = new();

    private ListCategoriesQueryHandler Handler => new(_db);

    private void Seed(params string[] names) =>
        _db.CategoryList.AddRange(names.Select(name => TestData.Category(name)));

    [Fact] // BR-C06, BR-L02
    public async Task Handle_ReturnsPageOrderedByName()
    {
        Seed("Work", "Home", "Study", "Errands");

        var result = await Handler.Handle(new ListCategoriesQuery(Page: 2, PageSize: 2), TestContext.Current.CancellationToken);

        var page = result.Value;
        page.Items.Select(c => c.Name).ShouldBe(["Study", "Work"]);
        page.Page.ShouldBe(2);
        page.PageSize.ShouldBe(2);
        page.TotalCount.ShouldBe(4);
        page.TotalPages.ShouldBe(2);
    }

    [Fact] // BR-L06
    public async Task Handle_WithSearch_FiltersByNameIgnoringCase()
    {
        Seed("Work", "Homework", "Home");

        var result = await Handler.Handle(new ListCategoriesQuery(Search: "  WORK "), TestContext.Current.CancellationToken);

        result.Value.Items.Select(c => c.Name).ShouldBe(["Homework", "Work"]);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WithBlankSearch_ReturnsAll()
    {
        Seed("Work", "Home");

        var result = await Handler.Handle(new ListCategoriesQuery(Search: "   "), TestContext.Current.CancellationToken);

        result.Value.TotalCount.ShouldBe(2);
    }
}
