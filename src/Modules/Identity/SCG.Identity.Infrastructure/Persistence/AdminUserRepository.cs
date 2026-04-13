using Microsoft.EntityFrameworkCore;
using SCG.Identity.Application.Abstractions;
using SCG.Identity.Domain.Entities;
using SCG.Identity.Infrastructure.Persistence;

namespace SCG.Identity.Infrastructure.Persistence;

internal sealed class AdminUserRepository : IAdminUserRepository
{
    private readonly IdentityDbContext _db;

    public AdminUserRepository(IdentityDbContext db) => _db = db;

    public Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.AdminUsers.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.AdminUsers.FirstOrDefaultAsync(a => a.Email == email, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => _db.AdminUsers.AnyAsync(a => a.Email == email, ct);

    public async Task AddAsync(AdminUser user, CancellationToken ct = default)
        => await _db.AdminUsers.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
