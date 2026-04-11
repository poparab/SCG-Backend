using SCG.Application.Abstractions.Messaging;

namespace SCG.Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;

public sealed record RefreshTokenResponse(string Token, string RefreshToken, string Email, string Role, Guid? AgencyId);
