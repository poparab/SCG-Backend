using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.ResetAdminPassword;

public sealed record ResetAdminPasswordCommand(
    Guid AdminUserId,
    string NewPassword) : ICommand;
