using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Queries.GetNationalities;

internal sealed class GetNationalitiesQueryHandler : IQueryHandler<GetNationalitiesQuery, PagedResult<NationalityDto>>
{
    private readonly INationalityRepository _repository;

    public GetNationalitiesQueryHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<NationalityDto>>> Handle(GetNationalitiesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetNationalitiesPagedAsync(
            request.SearchTerm,
            request.RequiresInquiry,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = new PagedResult<NationalityDto>(items, totalCount, request.Page, request.PageSize);
        return Result<PagedResult<NationalityDto>>.Success(result);
    }
}
