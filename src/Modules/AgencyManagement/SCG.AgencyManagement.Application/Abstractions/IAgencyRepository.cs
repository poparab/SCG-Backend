using SCG.AgencyManagement.Domain.Entities;
using SCG.AgencyManagement.Domain.Enums;

namespace SCG.AgencyManagement.Application.Abstractions;

public interface IAgencyRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByCommercialRegAsync(string commercialRegNumber, CancellationToken ct = default);
    Task<Agency?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Agency?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Agency agency, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<List<Agency>> GetAllAsync(string? searchTerm, AgencyStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? searchTerm, AgencyStatus? status, CancellationToken ct = default);
}
