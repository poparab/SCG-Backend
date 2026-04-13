using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.ToggleAdminActive;

public sealed record ToggleAdminActiveCommand(Guid AdminUserId) : ICommand;
