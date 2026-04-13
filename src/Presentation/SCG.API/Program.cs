using Asp.Versioning;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SCG.AgencyManagement.Infrastructure;
using SCG.API.Filters;
using SCG.Application.Abstractions.Behaviors;
using SCG.Identity.Infrastructure;
using SCG.InquiryManagement.Infrastructure;
using SCG.Infrastructure.Common;
using SCG.Infrastructure.Common.Middleware;
using SCG.Notification.Infrastructure;
using SCG.Rules.Infrastructure;
using Serilog;
using System.Text;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
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
    var jwtCookieName = "scg_auth";
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

            // Read JWT from HttpOnly cookie if no Authorization header
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (string.IsNullOrEmpty(context.Token) &&
                        context.Request.Cookies.TryGetValue(jwtCookieName, out var cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // -- MediatR (CQRS-light)
    var agencyAppAssembly = typeof(SCG.AgencyManagement.Application.Commands.RegisterAgency.RegisterAgencyCommand).Assembly;
    var identityAppAssembly = typeof(SCG.Identity.Application.Commands.Login.LoginCommand).Assembly;
    var rulesAppAssembly = typeof(SCG.Rules.Application.Commands.AddNationality.AddNationalityCommand).Assembly;
    var inquiryAppAssembly = typeof(SCG.InquiryManagement.Application.Commands.CreateBatch.CreateBatchCommand).Assembly;

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(
            agencyAppAssembly,
            identityAppAssembly,
            rulesAppAssembly,
            inquiryAppAssembly,
            typeof(SCG.AgencyManagement.Infrastructure.AgencyManagementServiceExtensions).Assembly,
            typeof(SCG.Identity.Infrastructure.IdentityServiceExtensions).Assembly,
            typeof(SCG.InquiryManagement.Infrastructure.InquiryManagementServiceExtensions).Assembly,
            typeof(SCG.Rules.Infrastructure.RulesServiceExtensions).Assembly,
            typeof(SCG.Notification.Infrastructure.NotificationServiceExtensions).Assembly,
            typeof(Program).Assembly
        );
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    // -- FluentValidation (validators from all module Application assemblies)
    builder.Services.AddValidatorsFromAssembly(agencyAppAssembly);
    builder.Services.AddValidatorsFromAssembly(identityAppAssembly);
    builder.Services.AddValidatorsFromAssembly(rulesAppAssembly);
    builder.Services.AddValidatorsFromAssembly(inquiryAppAssembly);
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    var isTesting = builder.Environment.IsEnvironment("Testing");

    // -- Hangfire (Background Processing)
    if (!isTesting)
    {
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
    }

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
    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ApiResponseFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // -- API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
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
    builder.Services.AddAuditSchema(connectionString);
    builder.Services.AddAgencyManagementModule(connectionString);
    builder.Services.AddIdentityModule(connectionString);
    builder.Services.AddInquiryManagementModule(connectionString);
    builder.Services.AddRulesModule(connectionString);
    builder.Services.AddNotificationModule(connectionString, builder.Configuration);

    // -- Rate Limiting (relaxed in Development/Staging for E2E testing)
    var isDevelopment = builder.Environment.IsDevelopment();
    var isDevelopmentLike = isDevelopment || isTesting || builder.Environment.IsStaging();
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter("auth", opt =>
        {
            opt.PermitLimit = isDevelopmentLike ? 1000 : 10;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        });

        options.AddSlidingWindowLimiter("api", opt =>
        {
            opt.PermitLimit = isDevelopmentLike ? 5000 : 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.SegmentsPerWindow = 4;
            opt.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("batch", opt =>
        {
            opt.PermitLimit = isDevelopmentLike ? 200 : 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        });
    });

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

    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self';";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        await next();
    });

    app.UseRateLimiter();
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

    if (!app.Environment.IsEnvironment("Testing") && (app.Environment.IsDevelopment() || app.Environment.IsStaging()))
    {
        app.UseHangfireDashboard("/hangfire");
    }

    if (!app.Environment.IsEnvironment("Testing"))
    {
        RecurringJob.AddOrUpdate<SCG.InquiryManagement.Infrastructure.Jobs.BatchStatusCheckJob>(
            "batch-status-check", job => job.ExecuteAsync(), "*/5 * * * *");
        RecurringJob.AddOrUpdate<SCG.Notification.Infrastructure.Jobs.NotificationDispatchJob>(
            "notification-dispatch", job => job.ExecuteAsync(), "*/2 * * * *");
        RecurringJob.AddOrUpdate<SCG.AgencyManagement.Infrastructure.Jobs.WalletLowBalanceAlertJob>(
            "wallet-low-balance", job => job.ExecuteAsync(), Cron.Daily());
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
