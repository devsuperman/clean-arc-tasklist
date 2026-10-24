using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Categories.Update;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Categories;

namespace TaskList.Application.UnitTests.Categories.Update;

public sealed class UpdateCategoryCommandHandlerTests
{
    private static readonly DateTimeOffset Later = TestData.Now.AddHours(1);

    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(Later);

    private UpdateCategoryCommandHandler Handler => new(_categories, _unitOfWork, _timeProvider);

    private Category Existing(string name = "Work")
    {
        var category = TestData.Category(name);
        _categories.GetByIdAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        return category;
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesCategoryAndSaves()
    {
        var category = Existing();

        var result = await Handler.Handle(
            new UpdateCategoryCommand(category.Id, " Office ", "Day job", "#abcdef"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        category.Name.ShouldBe("Office");
        category.Description.ShouldBe("Day job");
        category.Color.ShouldBe("#ABCDEF");
        category.UpdatedAt.ShouldBe(Later);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-C02
    public async Task Handle_RenamingToOwnNameInOtherCasing_ExcludesItselfFromUniquenessCheck()
    {
        var category = Existing("Work");

        var result = await Handler.Handle(new UpdateCategoryCommand(category.Id, "WORK", null, null), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        await _categories.Received(1).NameExistsAsync("WORK", category.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var result = await Handler.Handle(new UpdateCategoryCommand(Guid.NewGuid(), "Work", null, null), TestContext.Current.CancellationToken);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Category.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-C01
    public async Task Handle_WhenNameUsedByAnotherCategory_ReturnsNameAlreadyExists()
    {
        var category = Existing("Work");
        _categories.NameExistsAsync("Home", category.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.Handle(new UpdateCategoryCommand(category.Id, "Home", null, null), TestContext.Current.CancellationToken);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Category.NameAlreadyExists");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsInput_ReturnsDomainErrorWithoutSaving()
    {
        var category = Existing();

        var result = await Handler.Handle(new UpdateCategoryCommand(category.Id, "Work", null, "red"), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(CategoryErrors.ColorInvalid);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
