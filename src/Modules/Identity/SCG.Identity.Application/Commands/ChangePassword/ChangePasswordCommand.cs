using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string Email,
    string CurrentPassword,
    string NewPassword,
    string LoginType) : ICommand;
