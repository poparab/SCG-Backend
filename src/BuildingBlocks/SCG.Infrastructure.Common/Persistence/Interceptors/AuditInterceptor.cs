using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text.Json;

namespace SCG.Infrastructure.Common.Persistence.Interceptors;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> SkippedTypes =
    [
        "OutboxMessage",
        "RefreshToken",
        "NotificationLog",
        "AuditLog"
    ];

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory scopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _scopeFactory = scopeFactory;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = eventData.Context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && !SkippedTypes.Contains(e.Entity.GetType().Name))
            .ToList();

        if (entries.Count == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = httpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())
                ?.CurrentValue?.ToString() ?? "unknown";

            string? oldValues = null;
            string? newValues = null;
            string action;

            switch (entry.State)
            {
                case EntityState.Added:
                    action = "Create";
                    newValues = SerializeProperties(entry.Properties.ToDictionary(
                        p => p.Metadata.Name,
                        p => p.CurrentValue));
                    break;

                case EntityState.Modified:
                    action = "Update";
                    oldValues = SerializeProperties(entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    newValues = SerializeProperties(entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                    break;

                case EntityState.Deleted:
                    action = "Delete";
                    oldValues = SerializeProperties(entry.Properties.ToDictionary(
                        p => p.Metadata.Name,
                        p => p.OriginalValue));
                    break;

                default:
                    continue;
            }

            auditLogs.Add(AuditLog.Create(userId, userEmail, action, entityType, entityId, oldValues, newValues, ipAddress));
        }

        if (auditLogs.Count > 0)
        {
            using var scope = _scopeFactory.CreateScope();
            var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
            auditDb.AuditLogs.AddRange(auditLogs);
            await auditDb.SaveChangesAsync(cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static string SerializeProperties(Dictionary<string, object?> properties)
        => JsonSerializer.Serialize(properties);
}
