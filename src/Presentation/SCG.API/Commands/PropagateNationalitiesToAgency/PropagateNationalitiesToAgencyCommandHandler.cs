using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Domain.Entities;
using SCG.Rules.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.API.Commands.PropagateNationalitiesToAgency;

internal sealed class PropagateNationalitiesToAgencyCommandHandler
    : ICommandHandler<PropagateNationalitiesToAgencyCommand>
{
    private readonly RulesDbContext _rulesDb;

    public PropagateNationalitiesToAgencyCommandHandler(RulesDbContext rulesDb)
    {
        _rulesDb = rulesDb;
    }

    public async Task<Result> Handle(PropagateNationalitiesToAgencyCommand request, CancellationToken cancellationToken)
    {
        var allNationalityIds = await _rulesDb.Nationalities
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (allNationalityIds.Count == 0)
            return Result.Success();

        var existingNationalityIds = await _rulesDb.AgencyNationalities
            .Where(an => an.AgencyId == request.AgencyId)
            .Select(an => an.NationalityId)
            .ToListAsync(cancellationToken);

        var existingSet = existingNationalityIds.ToHashSet();

        var newRecords = allNationalityIds
            .Where(natId => !existingSet.Contains(natId))
            .Select(natId => AgencyNationality.Create(request.AgencyId, natId))
            .ToList();

        if (newRecords.Count > 0)
        {
            await _rulesDb.AgencyNationalities.AddRangeAsync(newRecords, cancellationToken);
            await _rulesDb.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
