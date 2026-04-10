using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Domain.Entities;
using SCG.AgencyManagement.Domain.Enums;
using SCG.AgencyManagement.Infrastructure.Persistence;

namespace SCG.AgencyManagement.Infrastructure.Persistence;

internal sealed class AgencyRepository : IAgencyRepository
{
    private readonly AgencyDbContext _db;

    public AgencyRepository(AgencyDbContext db) => _db = db;

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Agencies.AnyAsync(a => a.Email == email, ct);

    public async Task<bool> ExistsByCommercialRegAsync(string commercialRegNumber, CancellationToken ct = default)
        => await _db.Agencies.AnyAsync(a => a.CommercialLicenseNumber == commercialRegNumber, ct);

    public async Task<Agency?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Agencies
            .Include(a => a.Users)
            .Include(a => a.Wallet)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Agency?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Agencies
            .Include(a => a.Users)
            .Include(a => a.Wallet)
            .FirstOrDefaultAsync(a => a.Email == email, ct);

    public async Task AddAsync(Agency agency, CancellationToken ct = default)
        => await _db.Agencies.AddAsync(agency, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task<List<Agency>> GetAllAsync(
        string? searchTerm, AgencyStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(searchTerm, status);

        return await query
            .Include(a => a.Wallet)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        string? searchTerm, AgencyStatus? status, CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(searchTerm, status);
        return await query.CountAsync(ct);
    }

    private IQueryable<Agency> BuildFilteredQuery(string? searchTerm, AgencyStatus? status)
    {
        var query = _db.Agencies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.NameEn.ToLower().Contains(term) ||
                a.NameAr.Contains(term) ||
                a.Email.ToLower().Contains(term) ||
                a.CommercialLicenseNumber.Contains(term));
        }

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return query;
    }
}
