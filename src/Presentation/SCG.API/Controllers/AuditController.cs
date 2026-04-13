using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SCG.Infrastructure.Common.Persistence;
using SCG.SharedKernel;

namespace SCG.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-logs")]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin,SuperAdmin")]
[EnableRateLimiting("api")]
public class AuditController : ControllerBase
{
    private readonly AuditDbContext _auditDb;

    public AuditController(AuditDbContext auditDb)
    {
        _auditDb = auditDb;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] string? userEmail,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _auditDb.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(userEmail))
            query = query.Where(a => a.UserEmail != null && a.UserEmail.Contains(userEmail));

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (dateFrom.HasValue)
            query = query.Where(a => a.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => a.Timestamp <= dateTo.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogResponse(
                a.Id,
                a.UserId,
                a.UserEmail,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.IpAddress,
                a.Timestamp))
            .ToListAsync(ct);

        return Ok(new PagedResult<AuditLogResponse>(items, totalCount, page, pageSize));
    }
}

public sealed record AuditLogResponse(
    Guid Id,
    string? UserId,
    string? UserEmail,
    string Action,
    string EntityType,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTime Timestamp);
