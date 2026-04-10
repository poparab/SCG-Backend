using SCG.SharedKernel;

namespace SCG.Rules.Domain.Entities;

public sealed class AgencyCategory : Entity<Guid>
{
    public string NameAr { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AgencyCategory() { } // EF

    public static AgencyCategory Create(string nameAr, string nameEn, string? descAr = null, string? descEn = null)
    {
        return new AgencyCategory
        {
            Id = Guid.NewGuid(),
            NameAr = nameAr,
            NameEn = nameEn,
            DescriptionAr = descAr,
            DescriptionEn = descEn
        };
    }

    public void Update(string nameAr, string nameEn, string? descAr, string? descEn)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descAr;
        DescriptionEn = descEn;
    }

    public void Deactivate() => IsActive = false;
}
