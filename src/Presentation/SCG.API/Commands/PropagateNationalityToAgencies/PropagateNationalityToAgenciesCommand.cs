using SCG.Application.Abstractions.Messaging;

namespace SCG.API.Commands.PropagateNationalityToAgencies;

public sealed record PropagateNationalityToAgenciesCommand(Guid NationalityId) : ICommand;
