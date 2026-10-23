using NSubstitute;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Categories.Delete;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Categories;

namespace TaskList.Application.UnitTests.Categories.Delete;

public sealed class DeleteCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private DeleteCategoryCommandHandler Handler => new(_categories, _tasks, _unitOfWork);

    private Category Existing(int taskCount)
    {
        var category = TestData.Category();
        _categories.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _tasks.CountByCategoryAsync(category.Id, Arg.Any<CancellationToken>()).Returns(taskCount);
        return category;
    }

    [Fact] // BR-C05
    public async Task Handle_WhenCategoryHasNoTasks_RemovesAndSaves()
    {
        var category = Existing(taskCount: 0);

        var result = await Handler.Handle(new DeleteCategoryCommand(category.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        _categories.Received(1).Remove(category);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-C04
    public async Task Handle_WhenCategoryHasTasks_ReturnsHasTasksWithCount()
    {
        var category = Existing(taskCount: 3);

        var result = await Handler.Handle(new DeleteCategoryCommand(category.Id), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(CategoryErrors.HasTasks(3));
        _categories.DidNotReceive().Remove(Arg.Any<Category>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var result = await Handler.Handle(new DeleteCategoryCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Category.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
