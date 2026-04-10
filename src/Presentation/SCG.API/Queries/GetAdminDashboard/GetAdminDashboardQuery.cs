using SCG.Application.Abstractions.Messaging;

namespace SCG.API.Queries.GetAdminDashboard;

public sealed record GetAdminDashboardQuery() : IQuery<AdminDashboardDto>;
