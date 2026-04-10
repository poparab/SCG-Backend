using SCG.InquiryManagement.Domain.Entities;

namespace SCG.InquiryManagement.Application.Abstractions;

public interface IBatchRepository
{
    Task<Batch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Batch?> GetByIdWithTravelersAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Batch batch, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
