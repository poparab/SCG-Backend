using SCG.Rules.Application.Queries.GetAgencyNationalities;
using SCG.Rules.Application.Queries.GetMasterNationalityList;
using SCG.Rules.Application.Queries.GetNationalities;
using SCG.Rules.Application.Queries.GetNationalityById;
using SCG.Rules.Application.Queries.GetPricingList;
using SCG.Rules.Domain.Entities;

namespace SCG.Rules.Application.Abstractions;

public interface INationalityRepository
{
    // Commands
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task<Nationality?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Nationality nationality, CancellationToken ct = default);
    Task AddPricingAsync(Pricing pricing, CancellationToken ct = default);
    Task<Pricing?> GetActivePricingForNationalityAsync(string nationalityCode, Guid inquiryTypeId, CancellationToken ct = default);
    Task<Guid> GetDefaultInquiryTypeIdAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    // AgencyNationality Commands
    Task<AgencyNationality?> GetAgencyNationalityAsync(Guid agencyId, Guid nationalityId, CancellationToken ct = default);

    // Queries
    Task<(List<NationalityDto> Items, int TotalCount)> GetNationalitiesPagedAsync(
        string? searchTerm, bool? requiresInquiry, int page, int pageSize, CancellationToken ct = default);
    Task<NationalityDetailDto?> GetNationalityDetailAsync(Guid id, CancellationToken ct = default);
    Task<List<PricingListItemDto>> GetPricingListAsync(string? nationalityCode, bool activeOnly, CancellationToken ct = default);

    // AgencyNationality Queries
    Task<IReadOnlyList<MasterNationalityDto>> GetMasterNationalityListAsync(CancellationToken ct = default);
    Task<List<AgencyNationalityDto>> GetAgencyNationalitiesAsync(Guid agencyId, CancellationToken ct = default);
}
