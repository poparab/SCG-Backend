using SCG.SharedKernel;

namespace SCG.InquiryManagement.Domain.Events;

public sealed record InquiryApprovedDomainEvent(Guid InquiryId, string ReferenceNumber) : IDomainEvent;
