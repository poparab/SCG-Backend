using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.Login;

public sealed record LoginCommand(string Email, string Password, string LoginType) : ICommand<LoginResponse>;

public sealed record LoginResponse(string Token, string Email, string Role, Guid? AgencyId);
