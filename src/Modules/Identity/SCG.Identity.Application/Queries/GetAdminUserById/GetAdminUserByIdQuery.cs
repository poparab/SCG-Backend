using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Queries.GetAdminUserById;

public sealed record GetAdminUserByIdQuery(Guid AdminUserId) : IQuery<AdminUserDetail>;

public sealed record AdminUserDetail(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
