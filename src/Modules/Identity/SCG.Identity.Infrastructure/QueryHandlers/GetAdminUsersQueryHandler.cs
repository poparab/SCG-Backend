using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Queries.GetAdminUsers;
using SCG.Identity.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.Identity.Infrastructure.QueryHandlers;

internal sealed class GetAdminUsersQueryHandler : IQueryHandler<GetAdminUsersQuery, PagedResult<AdminUserResponse>>
{
    private readonly IdentityDbContext _db;

    public GetAdminUsersQueryHandler(IdentityDbContext db) => _db = db;

    public async Task<Result<PagedResult<AdminUserResponse>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.AdminUsers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.FullName.ToLower().Contains(search) ||
                a.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
            query = query.Where(a => a.Role == request.Role);

        if (request.IsActive.HasValue)
            query = query.Where(a => a.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AdminUserResponse(a.Id, a.FullName, a.Email, a.Role, a.IsActive, a.CreatedAt, a.LastLoginAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<AdminUserResponse>>.Success(
            new PagedResult<AdminUserResponse>(items, totalCount, request.Page, request.PageSize));
    }
}
