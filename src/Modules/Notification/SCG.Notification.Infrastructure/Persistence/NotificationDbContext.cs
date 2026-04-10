using Microsoft.EntityFrameworkCore;
using SCG.Infrastructure.Common.Persistence;
using SCG.Notification.Domain.Entities;

namespace SCG.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notification");
        base.OnModelCreating(modelBuilder);
    }
}
