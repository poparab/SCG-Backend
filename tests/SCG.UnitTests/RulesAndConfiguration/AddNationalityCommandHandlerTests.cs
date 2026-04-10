using SCG.Rules.Application.Abstractions;
using SCG.Rules.Application.Commands.AddNationality;
using SCG.Rules.Domain.Entities;

namespace SCG.UnitTests.RulesAndConfiguration;

public sealed class AddNationalityCommandHandlerTests
{
    private readonly INationalityRepository _repository = Substitute.For<INationalityRepository>();
    private readonly AddNationalityCommandHandler _sut;

    public AddNationalityCommandHandlerTests()
    {
        _sut = new AddNationalityCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidNationality_CreatesSuccessfully()
    {
        // Arrange
        _repository.ExistsByCodeAsync("US", Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddNationalityCommand("us", "الولايات المتحدة", "United States", false, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<Nationality>(n => n.Code == "US"), // code should be uppercased
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateCode_ReturnsFailure()
    {
        // Arrange
        _repository.ExistsByCodeAsync("US", Arg.Any<CancellationToken>()).Returns(true);

        var command = new AddNationalityCommand("US", "الولايات المتحدة", "United States", false, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Nationality with this code already exists.");
    }

    [Fact]
    public async Task Handle_RequiresInquiryWithFee_CreatesPricingEntry()
    {
        // Arrange
        var inquiryTypeId = Guid.NewGuid();
        _repository.ExistsByCodeAsync("SY", Arg.Any<CancellationToken>()).Returns(false);
        _repository.GetDefaultInquiryTypeIdAsync(Arg.Any<CancellationToken>()).Returns(inquiryTypeId);

        var command = new AddNationalityCommand("sy", "سوريا", "Syria", true, 150m);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddPricingAsync(
            Arg.Is<Pricing>(p => p.Fee == 150m && p.NationalityCode == "SY"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RequiresInquiryWithZeroFee_DoesNotCreatePricing()
    {
        // Arrange
        _repository.ExistsByCodeAsync("IQ", Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddNationalityCommand("iq", "العراق", "Iraq", true, 0);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.DidNotReceive().AddPricingAsync(Arg.Any<Pricing>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UppercasesNationalityCode()
    {
        // Arrange
        _repository.ExistsByCodeAsync("EG", Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddNationalityCommand("eg", "مصر", "Egypt", false, 0);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<Nationality>(n => n.Code == "EG"),
            Arg.Any<CancellationToken>());
    }
}
