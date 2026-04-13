using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SCG.Infrastructure.Common.Persistence.Interceptors;
using SCG.Notification.Application.Services;
using SCG.Notification.Infrastructure.Configuration;
using SCG.Notification.Infrastructure.Persistence;
using SCG.Notification.Infrastructure.Services;

namespace SCG.Notification.Infrastructure;

public static class NotificationServiceExtensions
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "notification");
            });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}
