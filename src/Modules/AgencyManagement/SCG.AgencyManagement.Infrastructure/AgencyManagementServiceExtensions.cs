using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.AgencyManagement.Infrastructure.Services;
using SCG.Application.Abstractions.Persistence;
using SCG.Application.Abstractions.Services;
using SCG.Infrastructure.Common.Persistence.Interceptors;

namespace SCG.AgencyManagement.Infrastructure;

public static class AgencyManagementServiceExtensions
{
    public static IServiceCollection AddAgencyManagementModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AgencyDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "agency");
            });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AgencyDbContext>());
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletService, WalletServiceAdapter>();

        return services;
    }
}
