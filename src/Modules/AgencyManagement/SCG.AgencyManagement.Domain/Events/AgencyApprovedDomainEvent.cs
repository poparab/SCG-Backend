using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Events;

public sealed record AgencyApprovedDomainEvent(Guid AgencyId) : IDomainEvent;
