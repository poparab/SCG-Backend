namespace SCG.Identity.Application.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string role, string? agencyId = null, string? fullName = null, string? agencyName = null);
}
