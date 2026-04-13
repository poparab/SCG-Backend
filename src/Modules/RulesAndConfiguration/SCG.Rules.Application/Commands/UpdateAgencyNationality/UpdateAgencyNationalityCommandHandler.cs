using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Application.Abstractions;
using SCG.Rules.Domain.Entities;
using SCG.SharedKernel;

namespace SCG.Rules.Application.Commands.UpdateAgencyNationality;

public sealed class UpdateAgencyNationalityCommandHandler
    : ICommandHandler<UpdateAgencyNationalityCommand>
{
    private readonly INationalityRepository _repository;

    public UpdateAgencyNationalityCommandHandler(INationalityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateAgencyNationalityCommand request, CancellationToken cancellationToken)
    {
        var agencyNationality = await _repository.GetAgencyNationalityAsync(
            request.AgencyId, request.NationalityId, cancellationToken);

        if (agencyNationality is null)
        {
            agencyNationality = AgencyNationality.Create(request.AgencyId, request.NationalityId);
            await _repository.AddAgencyNationalityAsync(agencyNationality, cancellationToken);
        }

        if (request.CustomFee.HasValue || request.CustomFee is null)
            agencyNationality.UpdateCustomFee(request.CustomFee);

        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value)
                agencyNationality.Enable();
            else
                agencyNationality.Disable();
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
