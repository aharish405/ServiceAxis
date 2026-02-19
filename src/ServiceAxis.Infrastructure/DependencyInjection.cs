using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Infrastructure.BackgroundJobs;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Infrastructure.Persistence.Repositories;
using ServiceAxis.Infrastructure.Services;

namespace ServiceAxis.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure-layer services with the DI container.
/// Call <c>services.AddInfrastructureServices(configuration)</c> from <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core ───────────────────────────────────────────────────────
        services.AddDbContext<ServiceAxisDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ServiceAxisDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

        // ── ASP.NET Identity ──────────────────────────────────────────────
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit           = true;
            options.Password.RequireLowercase        = true;
            options.Password.RequireUppercase        = true;
            options.Password.RequireNonAlphanumeric  = true;
            options.Password.RequiredLength          = 8;
            options.User.RequireUniqueEmail          = true;
        })
        .AddEntityFrameworkStores<ServiceAxisDbContext>()
        .AddDefaultTokenProviders();

        // ── Repository / Unit of Work ─────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // ── Infrastructure services ───────────────────────────────────────
        services.AddMemoryCache();


        // ─── Metadata & Record Engine ───
        services.AddScoped<ISysTableRepository, SysTableRepository>();
        services.AddScoped<ISysFieldRepository, SysFieldRepository>();
        services.AddScoped<IRecordRepository, RecordRepository>();
        services.AddScoped<IRecordValueRepository, RecordValueRepository>();

        // ─── Platform Services ───
        services.AddScoped<IFormEngineService, FormEngineService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISlaService, SlaService>();
        services.AddScoped<IAssignmentService, AssignmentService>();


        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddScoped<IEmailService, LoggingEmailService>();
        services.AddScoped<ISmsService, LoggingSmsService>();

        // ── Hangfire ──────────────────────────────────────────────────────
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout       = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout   = TimeSpan.FromMinutes(5),
                    QueuePollInterval            = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks           = true
                }));

        services.AddHangfireServer();

        // ── Hangfire background job types ─────────────────────────────────
        services.AddScoped<PlatformHealthCheckJob>();
        services.AddScoped<AuditLogCleanupJob>();

        return services;
    }
}
