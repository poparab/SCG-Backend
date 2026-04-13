using SCG.Application.Abstractions.Messaging;
using SCG.SharedKernel;

namespace SCG.Identity.Application.Queries.GetAdminUsers;

public sealed record GetAdminUsersQuery(
    string? SearchTerm,
    string? Role,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<AdminUserResponse>>;
