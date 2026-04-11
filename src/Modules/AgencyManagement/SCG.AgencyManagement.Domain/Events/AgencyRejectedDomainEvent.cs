using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Events;

public sealed record AgencyRejectedDomainEvent(Guid AgencyId, string Reason) : IDomainEvent;
