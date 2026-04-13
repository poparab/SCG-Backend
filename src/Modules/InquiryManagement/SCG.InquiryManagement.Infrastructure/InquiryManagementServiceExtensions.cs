using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.Application.Abstractions.Persistence;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.InquiryManagement.Infrastructure.Services;
using SCG.Infrastructure.Common.Persistence.Interceptors;

namespace SCG.InquiryManagement.Infrastructure;

public static class InquiryManagementServiceExtensions
{
    public static IServiceCollection AddInquiryManagementModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<InquiryDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "inquiry");
            });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IInquiryRepository, InquiryRepository>();
        services.AddScoped<IAgencyEmailResolver, AgencyEmailResolver>();

        return services;
    }
}
