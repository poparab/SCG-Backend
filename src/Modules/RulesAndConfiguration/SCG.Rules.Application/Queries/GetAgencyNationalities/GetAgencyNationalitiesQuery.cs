using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Queries.GetAgencyNationalities;

public sealed record GetAgencyNationalitiesQuery(Guid AgencyId) : IQuery<List<AgencyNationalityDto>>;
