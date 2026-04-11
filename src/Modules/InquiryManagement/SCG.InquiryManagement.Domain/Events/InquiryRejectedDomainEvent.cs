using SCG.SharedKernel;

namespace SCG.InquiryManagement.Domain.Events;

public sealed record InquiryRejectedDomainEvent(Guid InquiryId, string ReferenceNumber, string Reason) : IDomainEvent;
