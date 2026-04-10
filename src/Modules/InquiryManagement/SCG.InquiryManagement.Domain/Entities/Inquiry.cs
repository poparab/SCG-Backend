using SCG.InquiryManagement.Domain.Enums;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Domain.Entities;

public sealed class Inquiry : AggregateRoot<Guid>
{
    public Guid? AgencyId { get; private set; }
    public Guid? BatchId { get; private set; }
    public Guid InquiryTypeId { get; private set; }
    public string ReferenceNumber { get; private set; } = default!;

    // Traveler info
    public string FirstNameEn { get; private set; } = default!;
    public string LastNameEn { get; private set; } = default!;
    public string? FirstNameAr { get; private set; }
    public string? LastNameAr { get; private set; }
    public string PassportNumber { get; private set; } = default!;
    public string NationalityCode { get; private set; } = default!;
    public DateTime DateOfBirth { get; private set; }
    public TravelerGender Gender { get; private set; }
    public DateTime TravelDate { get; private set; }
    public string? ArrivalAirport { get; private set; }
    public string? TransitCountries { get; private set; }

    // Status & result
    public InquiryStatus Status { get; private set; } = InquiryStatus.Draft;
    public string? ResultCode { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? DocumentUrl { get; private set; }

    // Payment
    public decimal Fee { get; private set; }
    public string? PaymentReference { get; private set; }

    private Inquiry() { } // EF

    public static Inquiry Create(
        Guid inquiryTypeId, string referenceNumber,
        string firstNameEn, string lastNameEn,
        string passportNumber, string nationalityCode,
        DateTime dateOfBirth, TravelerGender gender,
        DateTime travelDate, decimal fee,
        Guid? agencyId = null, Guid? batchId = null)
    {
        return new Inquiry
        {
            Id = Guid.NewGuid(),
            InquiryTypeId = inquiryTypeId,
            ReferenceNumber = referenceNumber,
            FirstNameEn = firstNameEn,
            LastNameEn = lastNameEn,
            PassportNumber = passportNumber,
            NationalityCode = nationalityCode,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            TravelDate = travelDate,
            Fee = fee,
            AgencyId = agencyId,
            BatchId = batchId,
            Status = InquiryStatus.Submitted
        };
    }

    public void MarkPaymentPending() => Status = InquiryStatus.PaymentPending;

    public void MarkUnderProcessing(string paymentReference)
    {
        Status = InquiryStatus.UnderProcessing;
        PaymentReference = paymentReference;
    }

    public void Approve(string? documentUrl)
    {
        Status = InquiryStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
        DocumentUrl = documentUrl;
    }

    public void Reject(string reason)
    {
        Status = InquiryStatus.Rejected;
        RejectionReason = reason;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = InquiryStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }
}
