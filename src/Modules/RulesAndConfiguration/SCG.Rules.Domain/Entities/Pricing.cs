using SCG.SharedKernel;

namespace SCG.Rules.Domain.Entities;

public sealed class Pricing : Entity<Guid>
{
    public Guid InquiryTypeId { get; private set; }
    public Guid? AgencyCategoryId { get; private set; }
    public Guid? AgencyId { get; private set; } // Agency-specific override
    public string? NationalityCode { get; private set; } // Nationality-specific pricing
    public decimal Fee { get; private set; }
    public string Currency { get; private set; } = "USD";
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public InquiryType InquiryType { get; private set; } = default!;

    private Pricing() { } // EF

    public static Pricing Create(
        Guid inquiryTypeId, decimal fee,
        DateTime effectiveFrom, DateTime? effectiveTo,
        Guid? agencyCategoryId = null, Guid? agencyId = null,
        string? nationalityCode = null)
    {
        return new Pricing
        {
            Id = Guid.NewGuid(),
            InquiryTypeId = inquiryTypeId,
            Fee = fee,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            AgencyCategoryId = agencyCategoryId,
            AgencyId = agencyId,
            NationalityCode = nationalityCode
        };
    }

    public void Update(decimal fee, DateTime effectiveFrom, DateTime? effectiveTo)
    {
        Fee = fee;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public void Deactivate() => IsActive = false;
}
