using SCG.AgencyManagement.Domain.Enums;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Queries.GetAgencies;

public sealed record GetAgenciesQuery(
    string? SearchTerm,
    AgencyStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<AgencyListItemDto>>;
