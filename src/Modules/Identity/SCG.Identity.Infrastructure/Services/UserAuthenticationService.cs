using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Identity.Application.Services;
using SCG.Identity.Infrastructure.Persistence;

namespace SCG.Identity.Infrastructure.Services;

internal sealed class UserAuthenticationService : IUserAuthenticationService
{
    private readonly AgencyDbContext _agencyDb;
    private readonly IdentityDbContext _identityDb;

    public UserAuthenticationService(AgencyDbContext agencyDb, IdentityDbContext identityDb)
    {
        _agencyDb = agencyDb;
        _identityDb = identityDb;
    }

    public async Task<AuthenticatedUser?> GetUserByEmailAsync(string email, string loginType, CancellationToken ct = default)
    {
        if (loginType.Equals("agency", StringComparison.OrdinalIgnoreCase))
        {
            var user = await _agencyDb.AgencyUsers
                .AsNoTracking()
                .Include(u => u.Agency)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);

            if (user is null)
                return null;

            var fullName = string.IsNullOrWhiteSpace(user.LastNameEn)
                ? user.FirstNameEn
                : $"{user.FirstNameEn} {user.LastNameEn}";
            return new AuthenticatedUser(
                user.Id,
                user.Email,
                user.Role.ToString(),
                user.PasswordHash,
                user.AgencyId,
                user.Agency.Status.ToString(),
                fullName,
                user.Agency.NameEn);
        }

        if (loginType.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            var admin = await _identityDb.AdminUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == email && a.IsActive, ct);

            if (admin is null)
                return null;

            return new AuthenticatedUser(
                admin.Id,
                admin.Email,
                admin.Role,
                admin.PasswordHash,
                null,
                null,
                null,
                null);
        }

        return null;
    }

    public async Task<AuthenticatedUser?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        // Try agency user first
        var agencyUser = await _agencyDb.AgencyUsers
            .AsNoTracking()
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);

        if (agencyUser is not null)
        {
            var fullName = string.IsNullOrWhiteSpace(agencyUser.LastNameEn)
                ? agencyUser.FirstNameEn
                : $"{agencyUser.FirstNameEn} {agencyUser.LastNameEn}";
            return new AuthenticatedUser(
                agencyUser.Id, agencyUser.Email,
                agencyUser.Role.ToString(), agencyUser.PasswordHash,
                agencyUser.AgencyId, agencyUser.Agency.Status.ToString(),
                fullName, agencyUser.Agency.NameEn);
        }

        // Try admin user
        var admin = await _identityDb.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == userId && a.IsActive, ct);

        if (admin is not null)
        {
            return new AuthenticatedUser(
                admin.Id, admin.Email, admin.Role,
                admin.PasswordHash, null, null, null, null);
        }

        return null;
    }
}
