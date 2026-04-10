using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Queries.GetNationalityById;

internal sealed class GetNationalityByIdQueryHandler : IQueryHandler<GetNationalityByIdQuery, NationalityDetailDto>
{
    private readonly INationalityRepository _repository;

    public GetNationalityByIdQueryHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<NationalityDetailDto>> Handle(GetNationalityByIdQuery request, CancellationToken cancellationToken)
    {
        var detail = await _repository.GetNationalityDetailAsync(request.Id, cancellationToken);

        if (detail is null)
            return Result<NationalityDetailDto>.Failure("Nationality not found.");

        return Result<NationalityDetailDto>.Success(detail);
    }
}
