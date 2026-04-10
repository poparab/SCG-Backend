using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Queries.GetNationalityById;

public sealed record GetNationalityByIdQuery(Guid Id) : IQuery<NationalityDetailDto>;
