using SCG.Application.Abstractions.Messaging;

namespace SCG.Rules.Application.Commands.UpdateAgencyNationality;

public sealed record UpdateAgencyNationalityCommand(
    Guid AgencyId,
    Guid NationalityId,
    decimal? CustomFee,
    bool? IsEnabled) : ICommand;
