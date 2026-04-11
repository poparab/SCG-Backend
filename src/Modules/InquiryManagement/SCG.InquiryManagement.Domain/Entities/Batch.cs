using SCG.InquiryManagement.Domain.Enums;
using SCG.InquiryManagement.Domain.Events;
using SCG.SharedKernel;

namespace SCG.InquiryManagement.Domain.Entities;

public sealed class Batch : AggregateRoot<Guid>
{
    public Guid AgencyId { get; private set; }
    public string Name { get; private set; } = default!;
    public Guid InquiryTypeId { get; private set; }
    public string? Notes { get; private set; }
    public BatchStatus Status { get; private set; } = BatchStatus.Draft;
    public int TravelerCount { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public string? PaymentReference { get; private set; }
    public decimal? TotalFee { get; private set; }

    private readonly List<BatchTraveler> _travelers = [];
    public IReadOnlyList<BatchTraveler> Travelers => _travelers.AsReadOnly();

    private Batch() { } // EF

    public static Batch Create(
        Guid agencyId, string name,
        Guid inquiryTypeId, string? notes,
        Guid createdByUserId)
    {
        return new Batch
        {
            Id = Guid.NewGuid(),
            AgencyId = agencyId,
            Name = name,
            InquiryTypeId = inquiryTypeId,
            Notes = notes,
            CreatedByUserId = createdByUserId,
            Status = BatchStatus.Draft
        };
    }

    public void AddTraveler(BatchTraveler traveler)
    {
        _travelers.Add(traveler);
        TravelerCount = _travelers.Count;
    }

    public void RemoveTraveler(Guid travelerId)
    {
        _travelers.RemoveAll(t => t.Id == travelerId);
        TravelerCount = _travelers.Count;
    }

    public void SubmitForReview()
    {
        Status = BatchStatus.PendingReview;
    }

    public void Approve(Guid reviewerUserId)
    {
        Status = BatchStatus.PendingPayment;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(Guid reviewerUserId)
    {
        Status = BatchStatus.Draft;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void MarkPaymentComplete(string paymentReference, decimal totalFee)
    {
        Status = BatchStatus.Processing;
        PaymentReference = paymentReference;
        TotalFee = totalFee;
        SubmittedAt = DateTime.UtcNow;
        RaiseDomainEvent(new BatchSubmittedDomainEvent(Id, AgencyId, TravelerCount));
    }

    public void MarkCompleted() => Status = BatchStatus.Completed;
    public void MarkPartiallyCompleted() => Status = BatchStatus.PartiallyCompleted;
    public void MarkFailed() => Status = BatchStatus.Failed;
}
