using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Queries.GetMasterNationalityList;

public sealed record GetMasterNationalityListQuery() : IQuery<IReadOnlyList<MasterNationalityDto>>;
