using TaskList.Application.Categories;
using TaskList.Application.Common.Paging;
using TaskList.Application.Tasks;
using TaskList.Application.UnitTests.TestDoubles;

namespace TaskList.Application.UnitTests.Common;

public sealed class ProjectionTests
{
    private sealed record PageQuery(int Page, int PageSize) : IPagedQuery;

    [Fact] // BR-C07
    public void CategoryResponse_CountsAllAndPendingTasks()
    {
        var db = new InMemoryReadDbContext();
        var work = TestData.Category("Work");
        var home = TestData.Category("Home");
        db.CategoryList.AddRange([work, home]);
        db.TaskList.AddRange([
            TestData.Task(work.Id),
            TestData.Task(work.Id, status: TaskStatus.InProgress),
            TestData.Task(work.Id, status: TaskStatus.Done),
            TestData.Task(home.Id, status: TaskStatus.Done),
        ]);

        var responses = db.Categories.SelectResponse(db.Tasks).ToDictionary(response => response.Name);

        responses["Work"].TaskCount.ShouldBe(3);
        responses["Work"].PendingTaskCount.ShouldBe(2);
        responses["Home"].TaskCount.ShouldBe(1);
        responses["Home"].PendingTaskCount.ShouldBe(0);
    }

    [Fact]
    public void TaskResponse_EmbedsCategoryAndCalculatesIsOverdue()
    {
        var db = new InMemoryReadDbContext();
        var category = TestData.Category("Study", "#1E90FF");
        db.CategoryList.Add(category);
        var overdue = TestData.Task(category.Id, title: "Overdue", dueDate: TestData.Today);
        var doneAndPastDue = TestData.Task(category.Id, title: "Done", status: TaskStatus.Done, dueDate: TestData.Today);
        db.TaskList.AddRange([overdue, doneAndPastDue]);

        var responses = db.Tasks.SelectResponse(db.Categories, TestData.Today.AddDays(1)).ToDictionary(r => r.Title);

        responses["Overdue"].IsOverdue.ShouldBeTrue();
        responses["Done"].IsOverdue.ShouldBeFalse();
        responses["Overdue"].Category.ShouldBe(new CategorySummary(category.Id, "Study", "#1E90FF"));
    }

    [Fact] // BR-L02
    public async Task ToPagedResultAsync_ReturnsPageAndMetadata()
    {
        var db = new InMemoryReadDbContext();
        var category = TestData.Category();
        db.CategoryList.Add(category);
        db.TaskList.AddRange(Enumerable.Range(1, 5).Select(i => TestData.Task(category.Id, title: $"Task {i}")));

        var page = await db.ToPagedResultAsync(
            db.Tasks.OrderBy(task => task.Title).Select(task => task.Title),
            new PageQuery(2, 2),
            TestContext.Current.CancellationToken);

        page.Items.ShouldBe(["Task 3", "Task 4"]);
        page.TotalCount.ShouldBe(5);
        page.TotalPages.ShouldBe(3);
        page.Page.ShouldBe(2);
        page.PageSize.ShouldBe(2);
    }
}
