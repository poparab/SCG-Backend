using SCG.SharedKernel;

namespace SCG.InquiryManagement.Domain.Events;

public sealed record BatchSubmittedDomainEvent(Guid BatchId, Guid AgencyId, int TravelerCount) : IDomainEvent;
