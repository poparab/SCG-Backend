using SCG.Application.Abstractions.Messaging;

namespace SCG.AgencyManagement.Application.Queries.GetWalletByAgencyId;

public sealed record GetWalletQuery(Guid AgencyId) : IQuery<WalletDto>;
