using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Application.Commands.CreditWallet;
using SCG.AgencyManagement.Domain.Entities;

namespace SCG.UnitTests.AgencyManagement;

public sealed class CreditWalletCommandHandlerTests
{
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();
    private readonly CreditWalletCommandHandler _sut;

    public CreditWalletCommandHandlerTests()
    {
        _sut = new CreditWalletCommandHandler(_walletRepository);
    }

    [Fact]
    public async Task Handle_ValidCredit_ReturnsNewBalanceAndTransactionId()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var wallet = Wallet.Create(agencyId);
        _walletRepository.GetByAgencyIdAsync(agencyId, Arg.Any<CancellationToken>()).Returns(wallet);

        var command = new CreditWalletCommand(agencyId, 500m, "Cash", "REF-001", "Top-up", null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.NewBalance.Should().Be(500m);
        result.Value.TransactionId.Should().NotBeEmpty();
        await _walletRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ZeroAmount_ReturnsFailure()
    {
        // Arrange
        var command = new CreditWalletCommand(Guid.NewGuid(), 0m, "Cash", "REF-001", null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Credit amount must be greater than zero.");
    }

    [Fact]
    public async Task Handle_NegativeAmount_ReturnsFailure()
    {
        // Arrange
        var command = new CreditWalletCommand(Guid.NewGuid(), -100m, "Cash", "REF-001", null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Credit amount must be greater than zero.");
    }

    [Fact]
    public async Task Handle_WalletNotFound_ReturnsFailure()
    {
        // Arrange
        _walletRepository.GetByAgencyIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var command = new CreditWalletCommand(Guid.NewGuid(), 100m, "Cash", "REF-001", null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Wallet not found for the specified agency.");
    }

    [Fact]
    public async Task Handle_MultipleCredits_AccumulatesBalance()
    {
        // Arrange
        var agencyId = Guid.NewGuid();
        var wallet = Wallet.Create(agencyId);
        _walletRepository.GetByAgencyIdAsync(agencyId, Arg.Any<CancellationToken>()).Returns(wallet);

        // Act — first credit
        await _sut.Handle(new CreditWalletCommand(agencyId, 300m, "Cash", "REF-001", null, null), CancellationToken.None);
        var result = await _sut.Handle(new CreditWalletCommand(agencyId, 200m, "BankTransfer", "REF-002", null, null), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.NewBalance.Should().Be(500m);
    }
}
