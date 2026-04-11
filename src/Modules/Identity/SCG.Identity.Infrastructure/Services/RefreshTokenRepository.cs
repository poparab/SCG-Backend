using Microsoft.EntityFrameworkCore;
using SCG.Identity.Application.Services;
using SCG.Identity.Domain.Entities;
using SCG.Identity.Infrastructure.Persistence;

namespace SCG.Identity.Infrastructure.Services;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _db;

    public RefreshTokenRepository(IdentityDbContext db) => _db = db;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, ct);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _db.RefreshTokens.AddAsync(refreshToken, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
