using SCG.SharedKernel;

namespace SCG.Rules.Domain.Entities;

public sealed class Nationality : AggregateRoot<Guid>
{
    public string Code { get; private set; } = default!; // ISO 3166-1 alpha-2
    public string NameAr { get; private set; } = default!;
    public string NameEn { get; private set; } = default!;
    public bool RequiresInquiry { get; private set; }

    private readonly List<NationalityInquiryType> _inquiryTypes = [];
    public IReadOnlyList<NationalityInquiryType> InquiryTypes => _inquiryTypes.AsReadOnly();

    private Nationality() { } // EF

    public static Nationality Create(string code, string nameAr, string nameEn, bool requiresInquiry)
    {
        return new Nationality
        {
            Id = Guid.NewGuid(),
            Code = code,
            NameAr = nameAr,
            NameEn = nameEn,
            RequiresInquiry = requiresInquiry
        };
    }

    public void ToggleInquiryRequirement(bool requires) => RequiresInquiry = requires;

    public void AddInquiryType(Guid inquiryTypeId)
    {
        if (_inquiryTypes.All(x => x.InquiryTypeId != inquiryTypeId))
            _inquiryTypes.Add(NationalityInquiryType.Create(Id, inquiryTypeId));
    }

    public void RemoveInquiryType(Guid inquiryTypeId)
    {
        _inquiryTypes.RemoveAll(x => x.InquiryTypeId == inquiryTypeId);
    }
}
