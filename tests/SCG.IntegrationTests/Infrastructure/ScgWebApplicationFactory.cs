using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using SCG.AgencyManagement.Infrastructure.Persistence;
using SCG.Identity.Infrastructure.Persistence;
using SCG.InquiryManagement.Infrastructure.Persistence;
using SCG.Rules.Infrastructure.Persistence;
using SCG.Notification.Infrastructure.Persistence;

namespace SCG.IntegrationTests.Infrastructure;

public sealed class ScgWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test@Strong!Password123")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var connectionString = _dbContainer.GetConnectionString();

            // Remove existing DbContext registrations and re-register with test container
            ReplaceDbContext<AgencyDbContext>(services, connectionString);
            ReplaceDbContext<IdentityDbContext>(services, connectionString);
            ReplaceDbContext<InquiryDbContext>(services, connectionString);
            ReplaceDbContext<RulesDbContext>(services, connectionString);
            ReplaceDbContext<NotificationDbContext>(services, connectionString);

            // Remove Hangfire server registration (not needed in tests)
            services.RemoveAll(typeof(Hangfire.BackgroundJobServer));
            services.RemoveAll(typeof(Hangfire.IGlobalConfiguration));

            // Build provider and apply migrations
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            ApplyMigrations<AgencyDbContext>(scopedServices);
            ApplyMigrations<IdentityDbContext>(scopedServices);
            ApplyMigrations<InquiryDbContext>(scopedServices);
            ApplyMigrations<RulesDbContext>(scopedServices);
            ApplyMigrations<NotificationDbContext>(scopedServices);
        });

        builder.UseEnvironment("Development");
    }

    private static void ReplaceDbContext<TContext>(IServiceCollection services, string connectionString)
        where TContext : DbContext
    {
        services.RemoveAll(typeof(DbContextOptions<TContext>));
        services.RemoveAll(typeof(TContext));

        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory")));
    }

    private static void ApplyMigrations<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        var context = serviceProvider.GetRequiredService<TContext>();
        context.Database.Migrate();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
