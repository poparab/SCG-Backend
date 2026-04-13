using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.Infrastructure.Common.Persistence;
using SCG.Infrastructure.Common.Persistence.Interceptors;

namespace SCG.Infrastructure.Common;

public static class AuditServiceExtensions
{
    public static IServiceCollection AddAuditSchema(this IServiceCollection services, string connectionString)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AuditDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "audit");
            }));

        return services;
    }
}
