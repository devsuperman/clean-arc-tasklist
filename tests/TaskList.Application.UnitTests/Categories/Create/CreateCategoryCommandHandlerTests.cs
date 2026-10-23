using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TaskList.Application.Abstractions.Data;
using TaskList.Application.Categories.Create;
using TaskList.Application.UnitTests.TestDoubles;
using TaskList.Domain.Categories;

namespace TaskList.Application.UnitTests.Categories.Create;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(TestData.Now);

    private CreateCategoryCommandHandler Handler => new(_categories, _unitOfWork, _timeProvider);

    [Fact]
    public async Task Handle_WithValidCommand_AddsCategoryAndSaves()
    {
        Category? added = null;
        await _categories.AddAsync(Arg.Do<Category>(category => added = category), Arg.Any<CancellationToken>());

        var result = await Handler.Handle(
            new CreateCategoryCommand("  Work  ", "Office", "#1e90ff"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        added.ShouldNotBeNull();
        result.Value.ShouldBe(added.Id);
        added.Name.ShouldBe("Work");
        added.Color.ShouldBe("#1E90FF"); // BR-C03
        added.CreatedAt.ShouldBe(TestData.Now);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact] // BR-C01
    public async Task Handle_WhenNameExists_ReturnsNameAlreadyExists()
    {
        _categories.NameExistsAsync("Work", null, Arg.Any<CancellationToken>()).Returns(true);

        var result = await Handler.Handle(new CreateCategoryCommand(" Work ", null, null), TestContext.Current.CancellationToken);

        result.Error.ShouldNotBeNull().Code.ShouldBe("Category.NameAlreadyExists");
        await _categories.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsInput_ReturnsDomainErrorWithoutSaving()
    {
        var result = await Handler.Handle(new CreateCategoryCommand("W", null, null), TestContext.Current.CancellationToken);

        result.Error.ShouldBe(CategoryErrors.NameInvalidLength);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
