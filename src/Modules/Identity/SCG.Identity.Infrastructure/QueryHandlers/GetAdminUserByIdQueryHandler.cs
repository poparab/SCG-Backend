using Microsoft.EntityFrameworkCore;
using SCG.Application.Abstractions.Messaging;
using SCG.Identity.Application.Queries.GetAdminUserById;
using SCG.Identity.Infrastructure.Persistence;
using SCG.SharedKernel;

namespace SCG.Identity.Infrastructure.QueryHandlers;

internal sealed class GetAdminUserByIdQueryHandler : IQueryHandler<GetAdminUserByIdQuery, AdminUserDetail>
{
    private readonly IdentityDbContext _db;

    public GetAdminUserByIdQueryHandler(IdentityDbContext db) => _db = db;

    public async Task<Result<AdminUserDetail>> Handle(GetAdminUserByIdQuery request, CancellationToken cancellationToken)
    {
        var admin = await _db.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AdminUserId, cancellationToken);

        if (admin is null)
            return Result<AdminUserDetail>.Failure("Admin user not found.");

        var detail = new AdminUserDetail(
            admin.Id,
            admin.FullName,
            admin.Email,
            admin.Role,
            admin.IsActive,
            admin.CreatedAt,
            admin.LastLoginAt);

        return Result<AdminUserDetail>.Success(detail);
    }
}
