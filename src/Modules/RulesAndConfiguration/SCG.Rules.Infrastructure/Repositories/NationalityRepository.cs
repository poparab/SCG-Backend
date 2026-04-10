using Microsoft.EntityFrameworkCore;
using SCG.Rules.Application.Abstractions;
using SCG.Rules.Application.Queries.GetAgencyNationalities;
using SCG.Rules.Application.Queries.GetMasterNationalityList;
using SCG.Rules.Application.Queries.GetNationalities;
using SCG.Rules.Application.Queries.GetNationalityById;
using SCG.Rules.Application.Queries.GetPricingList;
using SCG.Rules.Domain.Entities;
using SCG.Rules.Infrastructure.Persistence;
using SCG.Rules.Infrastructure.Persistence.Seeds;

namespace SCG.Rules.Infrastructure.Repositories;

internal sealed class NationalityRepository : INationalityRepository
{
    private readonly RulesDbContext _context;

    public NationalityRepository(RulesDbContext context)
    {
        _context = context;
    }

    // -- Commands --

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct)
    {
        return await _context.Nationalities
            .AnyAsync(n => n.Code == code.ToUpperInvariant(), ct);
    }

    public async Task<Nationality?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Nationalities
            .Include(n => n.InquiryTypes)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task AddAsync(Nationality nationality, CancellationToken ct)
    {
        await _context.Nationalities.AddAsync(nationality, ct);
    }

    public async Task AddPricingAsync(Pricing pricing, CancellationToken ct)
    {
        await _context.Pricings.AddAsync(pricing, ct);
    }

    public async Task<Pricing?> GetActivePricingForNationalityAsync(string nationalityCode, Guid inquiryTypeId, CancellationToken ct)
    {
        return await _context.Pricings
            .FirstOrDefaultAsync(p =>
                p.NationalityCode == nationalityCode
                && p.InquiryTypeId == inquiryTypeId
                && p.IsActive
                && p.AgencyId == null
                && p.AgencyCategoryId == null, ct);
    }

    public async Task<Guid> GetDefaultInquiryTypeIdAsync(CancellationToken ct)
    {
        var inquiryType = await _context.InquiryTypes
            .FirstOrDefaultAsync(t => t.IsActive, ct);

        if (inquiryType is null)
            throw new InvalidOperationException("No active inquiry type found. Seed data may be missing.");

        return inquiryType.Id;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }

    // -- Queries --

    public async Task<(List<NationalityDto> Items, int TotalCount)> GetNationalitiesPagedAsync(
        string? searchTerm, bool? requiresInquiry, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Nationalities.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(n =>
                n.Code.ToLower().Contains(term)
                || n.NameEn.ToLower().Contains(term)
                || n.NameAr.Contains(term));
        }

        if (requiresInquiry.HasValue)
        {
            query = query.Where(n => n.RequiresInquiry == requiresInquiry.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var now = DateTime.UtcNow;

        var items = await query
            .OrderBy(n => n.NameEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .GroupJoin(
                _context.Pricings.AsNoTracking()
                    .Where(p => p.IsActive && p.AgencyId == null && p.AgencyCategoryId == null),
                n => n.Code,
                p => p.NationalityCode,
                (n, pricings) => new { Nationality = n, Pricings = pricings })
            .SelectMany(
                x => x.Pricings.DefaultIfEmpty(),
                (x, pricing) => new NationalityDto(
                    x.Nationality.Id,
                    x.Nationality.Code,
                    x.Nationality.NameAr,
                    x.Nationality.NameEn,
                    x.Nationality.RequiresInquiry,
                    pricing != null ? pricing.Fee : null,
                    x.Nationality.CreatedAt))
            .ToListAsync(ct);

        // Deduplicate — take the row with the highest fee (latest active pricing) per nationality
        var deduplicated = items
            .GroupBy(x => x.Id)
            .Select(g => g.OrderByDescending(x => x.CurrentFee).First())
            .ToList();

        return (deduplicated, totalCount);
    }

    public async Task<NationalityDetailDto?> GetNationalityDetailAsync(Guid id, CancellationToken ct)
    {
        var nationality = await _context.Nationalities.AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, ct);

        if (nationality is null)
            return null;

        var pricingHistory = await _context.Pricings.AsNoTracking()
            .Where(p => p.NationalityCode == nationality.Code
                && p.AgencyId == null
                && p.AgencyCategoryId == null)
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => new PricingDto(
                p.Id,
                p.Fee,
                p.Currency,
                p.EffectiveFrom,
                p.EffectiveTo,
                p.IsActive))
            .ToListAsync(ct);

        return new NationalityDetailDto(
            nationality.Id,
            nationality.Code,
            nationality.NameAr,
            nationality.NameEn,
            nationality.RequiresInquiry,
            pricingHistory,
            nationality.CreatedAt);
    }

    public async Task<List<PricingListItemDto>> GetPricingListAsync(
        string? nationalityCode, bool activeOnly, CancellationToken ct)
    {
        var query = _context.Pricings.AsNoTracking()
            .Where(p => p.AgencyId == null && p.AgencyCategoryId == null && p.NationalityCode != null);

        if (!string.IsNullOrWhiteSpace(nationalityCode))
        {
            query = query.Where(p => p.NationalityCode == nationalityCode.ToUpperInvariant());
        }

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var rawItems = await query
            .Join(
                _context.Nationalities.AsNoTracking(),
                p => p.NationalityCode,
                n => n.Code,
                (p, n) => new
                {
                    p.Id,
                    NatCode = n.Code,
                    n.NameEn,
                    n.NameAr,
                    p.Fee,
                    p.Currency,
                    p.EffectiveFrom,
                    p.EffectiveTo,
                    p.IsActive
                })
            .OrderBy(x => x.NameEn)
            .ToListAsync(ct);

        return rawItems.Select(x => new PricingListItemDto(
            x.Id, x.NatCode, x.NameEn, x.NameAr,
            x.Fee, x.Currency, x.EffectiveFrom, x.EffectiveTo, x.IsActive))
            .ToList();
    }

    // -- AgencyNationality --

    public async Task<AgencyNationality?> GetAgencyNationalityAsync(Guid agencyId, Guid nationalityId, CancellationToken ct)
    {
        return await _context.AgencyNationalities
            .FirstOrDefaultAsync(an => an.AgencyId == agencyId && an.NationalityId == nationalityId, ct);
    }

    public Task<IReadOnlyList<MasterNationalityDto>> GetMasterNationalityListAsync(CancellationToken ct)
    {
        IReadOnlyList<MasterNationalityDto> items = MasterNationalityList.GetAll()
            .Select(x => new MasterNationalityDto(x.Code, x.NameEn, x.NameAr))
            .ToList();

        return Task.FromResult(items);
    }

    public async Task<List<AgencyNationalityDto>> GetAgencyNationalitiesAsync(Guid agencyId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var records = await _context.AgencyNationalities
            .AsNoTracking()
            .Where(an => an.AgencyId == agencyId)
            .Include(an => an.Nationality)
            .ToListAsync(ct);

        var nationalityCodes = records.Select(r => r.Nationality.Code).Distinct().ToList();

        var defaultFees = await _context.Pricings
            .AsNoTracking()
            .Where(p =>
                p.IsActive
                && p.AgencyId == null
                && p.AgencyCategoryId == null
                && nationalityCodes.Contains(p.NationalityCode!))
            .GroupBy(p => p.NationalityCode)
            .Select(g => new { Code = g.Key, Fee = g.OrderByDescending(p => p.EffectiveFrom).First().Fee })
            .ToListAsync(ct);

        var feeMap = defaultFees.ToDictionary(x => x.Code!, x => x.Fee);

        return records.Select(an =>
        {
            var defaultFee = feeMap.GetValueOrDefault(an.Nationality.Code, 0m);
            var effectiveFee = an.CustomFee ?? defaultFee;

            return new AgencyNationalityDto(
                an.Id,
                an.AgencyId,
                an.NationalityId,
                an.Nationality.Code,
                an.Nationality.NameAr,
                an.Nationality.NameEn,
                an.Nationality.RequiresInquiry,
                defaultFee,
                an.CustomFee,
                effectiveFee,
                an.IsEnabled);
        })
        .OrderBy(x => x.NationalityNameEn)
        .ToList();
    }
}
