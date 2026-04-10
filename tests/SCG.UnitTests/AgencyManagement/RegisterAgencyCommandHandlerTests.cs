using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Application.Commands.RegisterAgency;
using SCG.Identity.Application.Services;

namespace SCG.UnitTests.AgencyManagement;

public sealed class RegisterAgencyCommandHandlerTests
{
    private readonly IAgencyRepository _repository = Substitute.For<IAgencyRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly RegisterAgencyCommandHandler _sut;

    public RegisterAgencyCommandHandlerTests()
    {
        _sut = new RegisterAgencyCommandHandler(_repository, _passwordHasher);
    }

    private static RegisterAgencyCommand ValidCommand() => new(
        AgencyName: "Test Agency",
        CommercialRegNumber: "CR-12345",
        ContactPersonName: "John Doe",
        Email: "test@agency.com",
        Password: "SecureP@ss1",
        CountryCode: "+20",
        MobileNumber: "01012345678");

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessWithAgencyId()
    {
        // Arrange
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_password");

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Any<SCG.AgencyManagement.Domain.Entities.Agency>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        _repository.ExistsByEmailAsync("test@agency.com", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("An agency with this email already exists.");
        await _repository.DidNotReceive().AddAsync(Arg.Any<SCG.AgencyManagement.Domain.Entities.Agency>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HashesPasswordBeforePersisting()
    {
        // Arrange
        _repository.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash("SecureP@ss1").Returns("hashed_value_123");

        // Act
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        _passwordHasher.Received(1).Hash("SecureP@ss1");
    }
}
