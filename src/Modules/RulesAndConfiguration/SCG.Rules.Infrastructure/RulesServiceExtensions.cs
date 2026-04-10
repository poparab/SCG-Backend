using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.Rules.Application.Abstractions;
using SCG.Rules.Infrastructure.Persistence;
using SCG.Rules.Infrastructure.Repositories;

namespace SCG.Rules.Infrastructure;

public static class RulesServiceExtensions
{
    public static IServiceCollection AddRulesModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RulesDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "rules");
            }));

        services.AddScoped<INationalityRepository, NationalityRepository>();

        return services;
    }
}
