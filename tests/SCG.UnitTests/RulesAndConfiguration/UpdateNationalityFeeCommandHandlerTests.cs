using SCG.Rules.Application.Abstractions;
using SCG.Rules.Application.Commands.UpdateNationalityFee;
using SCG.Rules.Domain.Entities;

namespace SCG.UnitTests.RulesAndConfiguration;

public sealed class UpdateNationalityFeeCommandHandlerTests
{
    private readonly INationalityRepository _repository = Substitute.For<INationalityRepository>();
    private readonly UpdateNationalityFeeCommandHandler _sut;

    public UpdateNationalityFeeCommandHandlerTests()
    {
        _sut = new UpdateNationalityFeeCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingNationality_CreatesNewPricingEntry()
    {
        // Arrange
        var nationality = Nationality.Create("US", "الولايات المتحدة", "United States", true);
        var inquiryTypeId = Guid.NewGuid();
        _repository.GetByIdAsync(nationality.Id, Arg.Any<CancellationToken>()).Returns(nationality);
        _repository.GetDefaultInquiryTypeIdAsync(Arg.Any<CancellationToken>()).Returns(inquiryTypeId);
        _repository.GetActivePricingForNationalityAsync("US", inquiryTypeId, Arg.Any<CancellationToken>())
            .Returns((Pricing?)null);

        var command = new UpdateNationalityFeeCommand(nationality.Id, 200m, DateTime.UtcNow, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddPricingAsync(
            Arg.Is<Pricing>(p => p.Fee == 200m),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingActivePricing_DeactivatesOldPricing()
    {
        // Arrange
        var nationality = Nationality.Create("US", "الولايات المتحدة", "United States", true);
        var inquiryTypeId = Guid.NewGuid();
        var oldPricing = Pricing.Create(inquiryTypeId, 100m, DateTime.UtcNow.AddMonths(-6), null, nationalityCode: "US");

        _repository.GetByIdAsync(nationality.Id, Arg.Any<CancellationToken>()).Returns(nationality);
        _repository.GetDefaultInquiryTypeIdAsync(Arg.Any<CancellationToken>()).Returns(inquiryTypeId);
        _repository.GetActivePricingForNationalityAsync("US", inquiryTypeId, Arg.Any<CancellationToken>())
            .Returns(oldPricing);

        var command = new UpdateNationalityFeeCommand(nationality.Id, 250m, DateTime.UtcNow, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        oldPricing.IsActive.Should().BeFalse();
        await _repository.Received(1).AddPricingAsync(
            Arg.Is<Pricing>(p => p.Fee == 250m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NationalityNotFound_ReturnsFailure()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Nationality?)null);

        var command = new UpdateNationalityFeeCommand(Guid.NewGuid(), 100m, DateTime.UtcNow, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Nationality not found.");
    }
}
