using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.AgencyManagement.Application.Abstractions;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Application.Abstractions.Persistence;

namespace SCG.AgencyManagement.Infrastructure;

public static class AgencyManagementServiceExtensions
{
    public static IServiceCollection AddAgencyManagementModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AgencyDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "agency");
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AgencyDbContext>());
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();

        return services;
    }
}
