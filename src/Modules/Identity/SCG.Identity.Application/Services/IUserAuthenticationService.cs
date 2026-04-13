namespace SCG.Identity.Application.Services;

public sealed record AuthenticatedUser(
    Guid UserId,
    string Email,
    string Role,
    string PasswordHash,
    Guid? AgencyId,
    string? AgencyStatus,
    string? FullName,
    string? AgencyName);

public interface IUserAuthenticationService
{
    Task<AuthenticatedUser?> GetUserByEmailAsync(string email, string loginType, CancellationToken ct = default);
    Task<AuthenticatedUser?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash, string loginType, CancellationToken ct = default);
}
