using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Services;
using SCG.Rules.Infrastructure.Persistence;

namespace SCG.Rules.Infrastructure.Services;

internal sealed class PricingServiceAdapter : IPricingService
{
    private readonly RulesDbContext _db;

    public PricingServiceAdapter(RulesDbContext db) => _db = db;

    public async Task<decimal?> GetFeeAsync(
        string nationalityCode, Guid inquiryTypeId, Guid? agencyId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Priority 1: Agency-specific custom fee from AgencyNationalities
        if (agencyId.HasValue)
        {
            var agencyNationality = await _db.AgencyNationalities
                .Include(an => an.Nationality)
                .Where(an => an.AgencyId == agencyId.Value
                             && an.Nationality.Code == nationalityCode
                             && an.IsEnabled
                             && an.CustomFee != null)
                .FirstOrDefaultAsync(ct);

            if (agencyNationality is not null)
                return agencyNationality.CustomFee!.Value;
        }

        // Priority 2: Default pricing (no agency, no category override)
        var pricing = await _db.Pricings
            .Where(p => p.InquiryTypeId == inquiryTypeId
                        && p.IsActive
                        && p.EffectiveFrom <= now
                        && (p.EffectiveTo == null || p.EffectiveTo > now)
                        && p.AgencyId == null
                        && p.AgencyCategoryId == null
                        && p.NationalityCode == nationalityCode)
            .FirstOrDefaultAsync(ct);

        return pricing?.Fee;
    }
}
