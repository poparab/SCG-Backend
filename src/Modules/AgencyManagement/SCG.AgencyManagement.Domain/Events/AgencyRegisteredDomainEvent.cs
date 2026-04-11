using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Events;

public sealed record AgencyRegisteredDomainEvent(Guid AgencyId, string Email) : IDomainEvent;
