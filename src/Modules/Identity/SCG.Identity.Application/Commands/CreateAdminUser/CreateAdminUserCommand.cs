using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.CreateAdminUser;

public sealed record CreateAdminUserCommand(
    string FullName,
    string Email,
    string Password,
    string Role) : ICommand<Guid>;
