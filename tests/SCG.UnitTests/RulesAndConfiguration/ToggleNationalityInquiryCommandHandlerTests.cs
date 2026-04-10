using SCG.Rules.Application.Abstractions;
using SCG.Rules.Application.Commands.ToggleNationalityInquiry;
using SCG.Rules.Domain.Entities;

namespace SCG.UnitTests.RulesAndConfiguration;

public sealed class ToggleNationalityInquiryCommandHandlerTests
{
    private readonly INationalityRepository _repository = Substitute.For<INationalityRepository>();
    private readonly ToggleNationalityInquiryCommandHandler _sut;

    public ToggleNationalityInquiryCommandHandlerTests()
    {
        _sut = new ToggleNationalityInquiryCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_EnableInquiry_SetsRequiresInquiryTrue()
    {
        // Arrange
        var nationality = Nationality.Create("US", "الولايات المتحدة", "United States", false);
        _repository.GetByIdAsync(nationality.Id, Arg.Any<CancellationToken>()).Returns(nationality);

        // Act
        var result = await _sut.Handle(
            new ToggleNationalityInquiryCommand(nationality.Id, true), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        nationality.RequiresInquiry.Should().BeTrue();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DisableInquiry_SetsRequiresInquiryFalse()
    {
        // Arrange
        var nationality = Nationality.Create("SY", "سوريا", "Syria", true);
        _repository.GetByIdAsync(nationality.Id, Arg.Any<CancellationToken>()).Returns(nationality);

        // Act
        var result = await _sut.Handle(
            new ToggleNationalityInquiryCommand(nationality.Id, false), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        nationality.RequiresInquiry.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NationalityNotFound_ReturnsFailure()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Nationality?)null);

        // Act
        var result = await _sut.Handle(
            new ToggleNationalityInquiryCommand(Guid.NewGuid(), true), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Nationality not found.");
    }
}
