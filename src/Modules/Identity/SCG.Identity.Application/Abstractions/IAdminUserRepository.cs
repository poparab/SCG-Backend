using SCG.Identity.Domain.Entities;

namespace SCG.Identity.Application.Abstractions;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(AdminUser user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
