using SCG.Identity.Application.Commands.Login;
using SCG.Identity.Application.Services;

namespace SCG.UnitTests.Identity;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserAuthenticationService _authService = Substitute.For<IUserAuthenticationService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(_authService, _passwordHasher, _jwtTokenGenerator, _refreshTokenRepo);
    }

    private static AuthenticatedUser AgencyUser(string status = "Approved") => new(
        Guid.NewGuid(), "user@agency.com", "Agency", "hashed_pwd",
        Guid.NewGuid(), status, "John Doe", "Test Agency");

    private static AuthenticatedUser AdminUser() => new(
        Guid.NewGuid(), "admin@scg.gov.eg", "SuperAdmin", "hashed_pwd",
        null, null, null, null);

    [Fact]
    public async Task Handle_ValidAgencyLogin_ReturnsTokenAndRole()
    {
        // Arrange
        var user = AgencyUser();
        _authService.GetUserByEmailAsync("user@agency.com", "agency", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("Pass@123", user.PasswordHash).Returns(true);
        _jwtTokenGenerator.GenerateToken(user.UserId, user.Email, user.Role, user.AgencyId.ToString(), user.FullName, user.AgencyName)
            .Returns("jwt_token_123");

        var command = new LoginCommand("user@agency.com", "Pass@123", "agency");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt_token_123");
        result.Value.Email.Should().Be("user@agency.com");
        result.Value.Role.Should().Be("Agency");
        result.Value.AgencyId.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidAdminLogin_ReturnsTokenWithSuperAdminRole()
    {
        // Arrange
        var user = AdminUser();
        _authService.GetUserByEmailAsync("admin@scg.gov.eg", "admin", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("Admin@1234", user.PasswordHash).Returns(true);
        _jwtTokenGenerator.GenerateToken(user.UserId, user.Email, user.Role, null, null, null)
            .Returns("admin_jwt_token");

        var command = new LoginCommand("admin@scg.gov.eg", "Admin@1234", "admin");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be("SuperAdmin");
        result.Value.AgencyId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsInvalidCredentials()
    {
        // Arrange
        _authService.GetUserByEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((AuthenticatedUser?)null);

        var command = new LoginCommand("nobody@test.com", "Pass@123", "agency");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsInvalidCredentials()
    {
        // Arrange
        var user = AgencyUser();
        _authService.GetUserByEmailAsync("user@agency.com", "agency", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("wrong_password", user.PasswordHash).Returns(false);

        var command = new LoginCommand("user@agency.com", "wrong_password", "agency");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_UnapprovedAgency_ReturnsAccountPendingApproval()
    {
        // Arrange
        var user = AgencyUser("PendingReview");
        _authService.GetUserByEmailAsync("user@agency.com", "agency", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("Pass@123", user.PasswordHash).Returns(true);

        var command = new LoginCommand("user@agency.com", "Pass@123", "agency");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account pending approval.");
    }

    [Fact]
    public async Task Handle_RejectedAgency_ReturnsAccountPendingApproval()
    {
        // Arrange
        var user = AgencyUser("Rejected");
        _authService.GetUserByEmailAsync("user@agency.com", "agency", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("Pass@123", user.PasswordHash).Returns(true);

        var command = new LoginCommand("user@agency.com", "Pass@123", "agency");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Account pending approval.");
    }
}
