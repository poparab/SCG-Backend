using SCG.InquiryManagement.Domain.Entities;

namespace SCG.InquiryManagement.Application.Abstractions;

public interface IInquiryRepository
{
    Task<Inquiry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Inquiry inquiry, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Inquiry> inquiries, CancellationToken ct = default);
    Task<int> CountOnDateAsync(DateTime date, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
