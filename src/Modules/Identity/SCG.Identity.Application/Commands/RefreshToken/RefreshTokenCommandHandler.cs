using System.Security.Cryptography;
using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Services;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IUserAuthenticationService _authService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepo,
        IUserAuthenticationService authService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _authService = authService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existing is null || !existing.IsActive)
            return Result<RefreshTokenResponse>.Failure("Invalid or expired refresh token.");

        var user = await _authService.GetUserByIdAsync(existing.UserId, cancellationToken);
        if (user is null)
            return Result<RefreshTokenResponse>.Failure("User not found.");

        // Revoke old token and create replacement
        var newRefreshTokenValue = GenerateRefreshToken();
        existing.Revoke("Replaced by new token", newRefreshTokenValue);

        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            user.UserId,
            newRefreshTokenValue,
            DateTime.UtcNow.AddDays(7));

        await _refreshTokenRepo.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepo.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateToken(
            user.UserId, user.Email, user.Role,
            user.AgencyId?.ToString(), user.FullName, user.AgencyName);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
            accessToken, newRefreshTokenValue, user.Email, user.Role, user.AgencyId));
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
