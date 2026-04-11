using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Queries.GetAgencyNationalities;

public sealed class GetAgencyNationalitiesQueryHandler
    : IQueryHandler<GetAgencyNationalitiesQuery, List<AgencyNationalityDto>>
{
    private readonly INationalityRepository _repository;

    public GetAgencyNationalitiesQueryHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AgencyNationalityDto>>> Handle(
        GetAgencyNationalitiesQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAgencyNationalitiesAsync(request.AgencyId, cancellationToken);
        return Result<List<AgencyNationalityDto>>.Success(items);
    }
}
