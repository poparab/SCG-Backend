using Microsoft.EntityFrameworkCore;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Domain.Entities;

namespace SCG.InquiryManagement.Infrastructure.Persistence;

internal sealed class BatchRepository : IBatchRepository
{
    private readonly InquiryDbContext _db;

    public BatchRepository(InquiryDbContext db) => _db = db;

    public async Task<Batch?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Batches.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<Batch?> GetByIdWithTravelersAsync(Guid id, CancellationToken ct = default)
        => await _db.Batches
            .Include(b => b.Travelers)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task AddAsync(Batch batch, CancellationToken ct = default)
        => await _db.Batches.AddAsync(batch, ct);

    public async Task<int> CountSubmittedOnDateAsync(DateTime date, CancellationToken ct = default)
        => await _db.Batches
            .CountAsync(b => b.SubmittedAt != null && b.SubmittedAt.Value.Date == date, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
