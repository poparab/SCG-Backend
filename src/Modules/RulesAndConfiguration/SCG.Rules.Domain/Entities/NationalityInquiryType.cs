using SCG.SharedKernel;

namespace SCG.Rules.Domain.Entities;

public sealed class NationalityInquiryType : Entity<Guid>
{
    public Guid NationalityId { get; private set; }
    public Guid InquiryTypeId { get; private set; }

    // Navigation
    public Nationality Nationality { get; private set; } = default!;
    public InquiryType InquiryType { get; private set; } = default!;

    private NationalityInquiryType() { } // EF

    internal static NationalityInquiryType Create(Guid nationalityId, Guid inquiryTypeId)
    {
        return new NationalityInquiryType
        {
            Id = Guid.NewGuid(),
            NationalityId = nationalityId,
            InquiryTypeId = inquiryTypeId
        };
    }
}
