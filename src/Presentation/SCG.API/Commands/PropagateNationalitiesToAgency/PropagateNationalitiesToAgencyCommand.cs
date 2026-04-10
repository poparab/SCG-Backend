using SCG.Application.Abstractions.Messaging;

namespace SCG.API.Commands.PropagateNationalitiesToAgency;

public sealed record PropagateNationalitiesToAgencyCommand(Guid AgencyId) : ICommand;
