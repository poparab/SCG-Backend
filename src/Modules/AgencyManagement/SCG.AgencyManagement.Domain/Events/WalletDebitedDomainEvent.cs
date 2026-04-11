using SCG.SharedKernel;

namespace SCG.AgencyManagement.Domain.Events;

public sealed record WalletDebitedDomainEvent(Guid WalletId, Guid AgencyId, decimal Amount, string Reference) : IDomainEvent;
