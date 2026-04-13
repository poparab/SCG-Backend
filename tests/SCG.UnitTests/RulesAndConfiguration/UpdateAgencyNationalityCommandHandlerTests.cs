using SCG.Rules.Application.Abstractions;
using SCG.Rules.Application.Commands.UpdateAgencyNationality;
using SCG.Rules.Domain.Entities;

namespace SCG.UnitTests.RulesAndConfiguration;

public sealed class UpdateAgencyNationalityCommandHandlerTests
{
    private readonly INationalityRepository _repository = Substitute.For<INationalityRepository>();
    private readonly UpdateAgencyNationalityCommandHandler _sut;

    public UpdateAgencyNationalityCommandHandlerTests()
    {
        _sut = new UpdateAgencyNationalityCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_MissingAgencyNationality_CreatesAndUpdatesRecord()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var nationalityId = Guid.NewGuid();

        _repository.GetAgencyNationalityAsync(agencyId, nationalityId, Arg.Any<CancellationToken>())
            .Returns((AgencyNationality?)null);

        var command = new UpdateAgencyNationalityCommand(agencyId, nationalityId, 175m, false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddAgencyNationalityAsync(
            Arg.Is<AgencyNationality>(an =>
                an.AgencyId == agencyId
                && an.NationalityId == nationalityId
                && an.CustomFee == 175m
                && an.IsEnabled == false),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingAgencyNationality_UpdatesWithoutCreatingNewRecord()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var nationalityId = Guid.NewGuid();
        var agencyNationality = AgencyNationality.Create(agencyId, nationalityId, 90m);
        agencyNationality.Disable();

        _repository.GetAgencyNationalityAsync(agencyId, nationalityId, Arg.Any<CancellationToken>())
            .Returns(agencyNationality);

        var command = new UpdateAgencyNationalityCommand(agencyId, nationalityId, 250m, true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        agencyNationality.CustomFee.Should().Be(250m);
        agencyNationality.IsEnabled.Should().BeTrue();
        await _repository.DidNotReceive().AddAgencyNationalityAsync(Arg.Any<AgencyNationality>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}