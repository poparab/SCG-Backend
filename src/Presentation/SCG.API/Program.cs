using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SCG.AgencyManagement.Infrastructure;
using SCG.Identity.Infrastructure;
using SCG.InquiryManagement.Infrastructure;
using SCG.Infrastructure.Common.Middleware;
using SCG.Notification.Infrastructure;
using SCG.Rules.Infrastructure;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

try
{
    Log.Information("Starting SCG API host");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // -- Authentication (JWT)
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SCG-Dev-Key-Must-Be-At-Least-32-Characters!"))
            };
        });

    builder.Services.AddAuthorization();

    // -- MediatR (CQRS-light)
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(SCG.AgencyManagement.Application.Commands.RegisterAgency.RegisterAgencyCommand).Assembly,
            typeof(SCG.Identity.Application.Commands.Login.LoginCommand).Assembly,
            typeof(SCG.Rules.Application.Commands.AddNationality.AddNationalityCommand).Assembly,
            typeof(SCG.InquiryManagement.Application.Commands.CreateBatch.CreateBatchCommand).Assembly,
            typeof(SCG.AgencyManagement.Infrastructure.AgencyManagementServiceExtensions).Assembly,
            typeof(SCG.Identity.Infrastructure.IdentityServiceExtensions).Assembly,
            typeof(SCG.InquiryManagement.Infrastructure.InquiryManagementServiceExtensions).Assembly,
            typeof(SCG.Rules.Infrastructure.RulesServiceExtensions).Assembly,
            typeof(SCG.Notification.Infrastructure.NotificationServiceExtensions).Assembly,
            typeof(Program).Assembly
        );
    });

    // -- Hangfire (Background Processing)
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            builder.Configuration.GetConnectionString("HangfireConnection"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                SchemaName = "hangfire"
            }));

    builder.Services.AddHangfireServer(options =>
    {
        options.Queues = ["express", "standard", "batch"];
        options.WorkerCount = Environment.ProcessorCount * 2;
    });

    // -- CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularDev", policy =>
        {
            var corsOrigins = builder.Configuration["CORS:Origins"];
            var origins = !string.IsNullOrEmpty(corsOrigins)
                ? corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : new[]
                {
                    "http://localhost:4200", "https://localhost:4200",
                    "http://localhost:4201", "https://localhost:4201",
                    "http://localhost:4203", "https://localhost:4203",
                    "http://localhost:4204", "https://localhost:4204"
                };
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // -- Controllers + Swagger
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Security Clearance Gateway API",
            Version = "v1",
            Description = "SCG Backend API - Modular Monolith"
        });
    });

    // -- Module Registration (EF Core DbContexts + DI)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddAgencyManagementModule(connectionString);
    builder.Services.AddIdentityModule(connectionString);
    builder.Services.AddInquiryManagementModule(connectionString);
    builder.Services.AddRulesModule(connectionString);
    builder.Services.AddNotificationModule(connectionString);

    // -- Health Checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            connectionString,
            name: "sqlserver",
            tags: ["db", "ready"]);

    var app = builder.Build();

    // -- Middleware Pipeline
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SCG API v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAngularDev");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseHangfireDashboard("/hangfire");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
