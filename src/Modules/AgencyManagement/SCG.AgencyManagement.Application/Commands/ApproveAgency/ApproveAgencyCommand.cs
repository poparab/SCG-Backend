using SCG.Application.Abstractions.Messaging;

namespace SCG.AgencyManagement.Application.Commands.ApproveAgency;

public sealed record ApproveAgencyCommand(Guid AgencyId) : ICommand;
