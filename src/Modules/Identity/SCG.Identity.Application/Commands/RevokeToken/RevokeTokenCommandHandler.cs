using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Services;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler : ICommandHandler<RevokeTokenCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public RevokeTokenCommandHandler(IRefreshTokenRepository refreshTokenRepo) =>
        _refreshTokenRepo = refreshTokenRepo;

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (token is null)
            return Result.Failure("Refresh token not found.");

        if (!token.IsActive)
            return Result.Success(); // Already revoked — idempotent

        token.Revoke("Revoked by user");
        await _refreshTokenRepo.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
