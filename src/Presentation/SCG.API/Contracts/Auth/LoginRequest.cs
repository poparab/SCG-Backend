namespace SCG.API.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password, string LoginType);
