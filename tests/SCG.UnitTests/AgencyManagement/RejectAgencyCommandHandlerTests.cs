using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Application.Commands.RejectAgency;
using SCG.AgencyManagement.Domain.Entities;
using SCG.AgencyManagement.Domain.Enums;

namespace SCG.UnitTests.AgencyManagement;

public sealed class RejectAgencyCommandHandlerTests
{
    private readonly IAgencyRepository _repository = Substitute.For<IAgencyRepository>();
    private readonly RejectAgencyCommandHandler _sut;

    public RejectAgencyCommandHandlerTests()
    {
        _sut = new RejectAgencyCommandHandler(_repository);
    }

    private static Agency CreatePendingAgency()
    {
        return Agency.Register(
            "Test Agency", "CR-123", "John Doe",
            "test@agency.com", "hashed_pwd", "+20", "01012345678");
    }

    [Fact]
    public async Task Handle_PendingAgency_RejectsWithReason()
    {
        // Arrange
        var agency = CreatePendingAgency();
        _repository.GetByIdAsync(agency.Id, Arg.Any<CancellationToken>()).Returns(agency);
        var command = new RejectAgencyCommand(agency.Id, "Invalid documents");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        agency.Status.Should().Be(AgencyStatus.Rejected);
        agency.RejectionReason.Should().Be("Invalid documents");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingAgency_ReturnsFailure()
    {
        // Arrange
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Agency?)null);

        // Act
        var result = await _sut.Handle(new RejectAgencyCommand(Guid.NewGuid(), "reason"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Agency not found.");
    }

    [Fact]
    public async Task Handle_AlreadyApprovedAgency_ReturnsFailure()
    {
        // Arrange
        var agency = CreatePendingAgency();
        agency.Approve();
        _repository.GetByIdAsync(agency.Id, Arg.Any<CancellationToken>()).Returns(agency);

        // Act
        var result = await _sut.Handle(new RejectAgencyCommand(agency.Id, "reason"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Only agencies with Pending Review status can be rejected.");
    }
}
