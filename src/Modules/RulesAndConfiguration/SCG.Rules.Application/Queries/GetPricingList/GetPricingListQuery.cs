using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Queries.GetPricingList;

public sealed record GetPricingListQuery(
    string? NationalityCode,
    bool ActiveOnly = true) : IQuery<IReadOnlyList<PricingListItemDto>>;
