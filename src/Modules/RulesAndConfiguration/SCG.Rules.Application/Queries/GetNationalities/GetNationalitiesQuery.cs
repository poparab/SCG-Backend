using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Queries.GetNationalities;

public sealed record GetNationalitiesQuery(
    string? SearchTerm,
    bool? RequiresInquiry,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<NationalityDto>>;
