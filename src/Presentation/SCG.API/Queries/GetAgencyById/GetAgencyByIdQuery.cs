using SCG.Application.Abstractions.Messaging;

namespace SCG.API.Queries.GetAgencyById;

public sealed record GetAgencyByIdQuery(Guid Id) : IQuery<AgencyDetailDto>;
