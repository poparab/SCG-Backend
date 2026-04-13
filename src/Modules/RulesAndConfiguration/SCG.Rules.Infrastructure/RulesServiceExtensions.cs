using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.Application.Abstractions.Services;
using SCG.Infrastructure.Common.Persistence.Interceptors;
using SCG.Rules.Application.Abstractions;
using SCG.Rules.Infrastructure.Persistence;
using SCG.Rules.Infrastructure.Repositories;
using SCG.Rules.Infrastructure.Services;

namespace SCG.Rules.Infrastructure;

public static class RulesServiceExtensions
{
    public static IServiceCollection AddRulesModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RulesDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "rules");
            });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<INationalityRepository, NationalityRepository>();
        services.AddScoped<IPricingService, PricingServiceAdapter>();

        return services;
    }
}
