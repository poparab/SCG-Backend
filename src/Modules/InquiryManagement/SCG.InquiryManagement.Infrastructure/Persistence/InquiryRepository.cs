using Microsoft.EntityFrameworkCore;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Entities;

namespace SCG.InquiryManagement.Infrastructure.Persistence;

internal sealed class InquiryRepository : IInquiryRepository
{
    private readonly InquiryDbContext _db;

    public InquiryRepository(InquiryDbContext db) => _db = db;

    public async Task<Inquiry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Inquiries.FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task AddAsync(Inquiry inquiry, CancellationToken ct = default)
        => await _db.Inquiries.AddAsync(inquiry, ct);

    public async Task AddRangeAsync(IEnumerable<Inquiry> inquiries, CancellationToken ct = default)
        => await _db.Inquiries.AddRangeAsync(inquiries, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
