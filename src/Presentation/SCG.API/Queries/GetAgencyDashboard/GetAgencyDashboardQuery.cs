using SCG.Application.Abstractions.Messaging;

namespace SCG.API.Queries.GetAgencyDashboard;

public sealed record GetAgencyDashboardQuery(Guid AgencyId) : IQuery<AgencyDashboardDto>;
