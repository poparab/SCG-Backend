using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.Application.Abstractions.Persistence;
using SCG.InquiryManagement.Application.Abstractions;
using SCG.InquiryManagement.Infrastructure.Persistence;

namespace SCG.InquiryManagement.Infrastructure;

public static class InquiryManagementServiceExtensions
{
    public static IServiceCollection AddInquiryManagementModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<InquiryDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "inquiry");
            }));

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IInquiryRepository, InquiryRepository>();

        return services;
    }
}
