using SCG.AgencyManagement.Application.Abstractions;
using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.AgencyManagement.Application.Queries.GetAgencies;

internal sealed class GetAgenciesQueryHandler : IQueryHandler<GetAgenciesQuery, PagedResult<AgencyListItemDto>>
{
    private readonly IAgencyRepository _repository;

    public GetAgenciesQueryHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<AgencyListItemDto>>> Handle(GetAgenciesQuery request, CancellationToken cancellationToken)
    {
        var agencies = await _repository.GetAllAsync(
            request.SearchTerm, request.Status, request.Page, request.PageSize, cancellationToken);

        var totalCount = await _repository.CountAsync(
            request.SearchTerm, request.Status, cancellationToken);

        var items = agencies.Select(a => new AgencyListItemDto(
            a.Id,
            a.NameEn,
            a.NameAr,
            a.Email,
            a.CommercialLicenseNumber,
            a.Status,
            a.Wallet?.Balance ?? 0m,
            a.CreatedAt)).ToList();

        return Result<PagedResult<AgencyListItemDto>>.Success(
            new PagedResult<AgencyListItemDto>(items, totalCount, request.Page, request.PageSize));
    }
}
