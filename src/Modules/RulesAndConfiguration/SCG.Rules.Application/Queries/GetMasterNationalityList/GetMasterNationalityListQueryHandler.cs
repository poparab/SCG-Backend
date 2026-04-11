using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Queries.GetMasterNationalityList;

public sealed class GetMasterNationalityListQueryHandler
    : IQueryHandler<GetMasterNationalityListQuery, IReadOnlyList<MasterNationalityDto>>
{
    private readonly INationalityRepository _repository;

    public GetMasterNationalityListQueryHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<MasterNationalityDto>>> Handle(
        GetMasterNationalityListQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetMasterNationalityListAsync(cancellationToken);
        return Result<IReadOnlyList<MasterNationalityDto>>.Success(items);
    }
}
