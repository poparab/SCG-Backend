using SCG.Identity.Domain.Entities;

namespace SCG.Identity.Application.Services;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
