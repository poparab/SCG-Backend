using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SCG.Identity.Application.Services;
using SCG.Identity.Infrastructure.Persistence;
using SCG.Identity.Infrastructure.Services;

namespace SCG.Identity.Infrastructure;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity");
            }));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

        return services;
    }
}
