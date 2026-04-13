using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.UpdateAdminUser;

public sealed record UpdateAdminUserCommand(
    Guid AdminUserId,
    string FullName,
    string Role) : ICommand;
