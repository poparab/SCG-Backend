using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.RevokeToken;

public sealed record RevokeTokenCommand(string RefreshToken) : ICommand;
