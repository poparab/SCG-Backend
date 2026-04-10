using Microsoft.EntityFrameworkCore;
using SCG.AgencyManagement.Domain.Enums;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Application.Abstractions.Messaging;
using SCG.Rules.Domain.Entities;
using SCG.Rules.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.API.Commands.PropagateNationalityToAgencies;

internal sealed class PropagateNationalityToAgenciesCommandHandler
    : ICommandHandler<PropagateNationalityToAgenciesCommand>
{
    private readonly AgencyDbContext _agencyDb;
    private readonly RulesDbContext _rulesDb;

    public PropagateNationalityToAgenciesCommandHandler(AgencyDbContext agencyDb, RulesDbContext rulesDb)
    {
        _agencyDb = agencyDb;
        _rulesDb = rulesDb;
    }

    public async Task<Result> Handle(PropagateNationalityToAgenciesCommand request, CancellationToken cancellationToken)
    {
        var nationalityExists = await _rulesDb.Nationalities
            .AnyAsync(n => n.Id == request.NationalityId, cancellationToken);

        if (!nationalityExists)
            return Result.Failure("Nationality not found.");

        var approvedAgencyIds = await _agencyDb.Agencies
            .AsNoTracking()
            .Where(a => a.Status == AgencyStatus.Approved)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        if (approvedAgencyIds.Count == 0)
            return Result.Success();

        var existingAgencyIds = await _rulesDb.AgencyNationalities
            .Where(an => an.NationalityId == request.NationalityId)
            .Select(an => an.AgencyId)
            .ToListAsync(cancellationToken);

        var existingSet = existingAgencyIds.ToHashSet();

        var newRecords = approvedAgencyIds
            .Where(agencyId => !existingSet.Contains(agencyId))
            .Select(agencyId => AgencyNationality.Create(agencyId, request.NationalityId))
            .ToList();

        if (newRecords.Count > 0)
        {
            await _rulesDb.AgencyNationalities.AddRangeAsync(newRecords, cancellationToken);
            await _rulesDb.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
