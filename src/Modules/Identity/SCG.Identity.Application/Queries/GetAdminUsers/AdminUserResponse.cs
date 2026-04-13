namespace SCG.Identity.Application.Queries.GetAdminUsers;

public sealed record AdminUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
