using SCG.Application.Abstractions.Messaging;

namespace SCG.AgencyManagement.Application.Commands.RejectAgency;

public sealed record RejectAgencyCommand(Guid AgencyId, string Reason) : ICommand;
