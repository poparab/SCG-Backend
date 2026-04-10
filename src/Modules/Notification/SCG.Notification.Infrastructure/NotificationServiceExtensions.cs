using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.Notification.Infrastructure.Persistence;

namespace SCG.Notification.Infrastructure;

public static class NotificationServiceExtensions
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "notification");
            }));

        return services;
    }
}
