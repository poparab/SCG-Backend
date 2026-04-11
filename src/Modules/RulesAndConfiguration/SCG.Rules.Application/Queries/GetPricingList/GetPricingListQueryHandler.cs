using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Queries.GetPricingList;

public sealed class GetPricingListQueryHandler : IQueryHandler<GetPricingListQuery, IReadOnlyList<PricingListItemDto>>
{
    private readonly INationalityRepository _repository;

    public GetPricingListQueryHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<PricingListItemDto>>> Handle(GetPricingListQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetPricingListAsync(
            request.NationalityCode,
            request.ActiveOnly,
            cancellationToken);

        return Result<IReadOnlyList<PricingListItemDto>>.Success(items);
    }
}
