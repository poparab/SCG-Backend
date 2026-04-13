namespace SCG.API.Contracts.Auth;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
