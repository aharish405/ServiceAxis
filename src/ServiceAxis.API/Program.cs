using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using ServiceAxis.API.Middleware;
using ServiceAxis.Application;
using ServiceAxis.Identity;
using ServiceAxis.Infrastructure;
using ServiceAxis.Infrastructure.BackgroundJobs;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Shared.Settings;

// ─── Bootstrap Serilog early ─────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ServiceAxis Platform...");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog (full) ────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ServiceAxis")
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/serviceaxis-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
    });

    // ─── Strongly-typed settings ───────────────────────────────────────────
    builder.Services.Configure<AppSettings>(
        builder.Configuration.GetSection(AppSettings.SectionName));
    builder.Services.Configure<JwtSettings>(
        builder.Configuration.GetSection(JwtSettings.SectionName));

    // ─── Layer registrations ───────────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);

    // ─── Controllers ───────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ─── Swagger / OpenAPI ─────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title       = "ServiceAxis API",
            Version     = "v1",
            Description = "Enterprise Service Management Platform — API Documentation"
        });

        // JWT auth in Swagger UI
        c.AddSecurityDefinition("Bearer", new()
        {
            Name         = "Authorization",
            Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description  = "Enter your JWT token. Example: eyJhbGci..."
        });
        c.AddSecurityRequirement(new()
        {
            {
                new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });

        // Include XML documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);
    });

    // ─── CORS ──────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultCors", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // ─── Health checks ─────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ServiceAxisDbContext>("database");

    // ═══════════════════════════════════════════════════════════════════════
    var app = builder.Build();

    // ─── Database auto-migrate + seed ─────────────────────────────────────
    // ─── Database auto-migrate + seed ─────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var logger      = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var db          = scope.ServiceProvider.GetRequiredService<ServiceAxisDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            logger.LogInformation("Applying EF Core migrations...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations applied. Seeding database...");
            await DbSeeder.SeedAsync(userManager, roleManager);
            await SecuritySeeder.SeedAsync(db, roleManager);
            await MetadataSeeder.SeedAsync(db);
            await LifecycleSeeder.SeedAsync(db);
            logger.LogInformation("Database ready.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            if (!app.Environment.IsDevelopment()) throw;
        }
    }

    // ─── Middleware pipeline ───────────────────────────────────────────────
    app.UseSerilogRequestLogging();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceAxis API v1");
            c.RoutePrefix = string.Empty; // Serve Swagger at root
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("DefaultCors");
    app.UseAuthentication();
    app.UseAuthorization();

    // ─── Hangfire dashboard ────────────────────────────────────────────────
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // TODO: Add authentication filter for production
        Authorization = []
    });

    // ─── Register recurring jobs ───────────────────────────────────────────
    RecurringJobRegistrations.Register();

    // ─── Endpoints ────────────────────────────────────────────────────────
    app.MapControllers();

    // Health check (minimal API — as per spec)
    app.MapGet("/health", () => Results.Ok(new
    {
        Status    = "Healthy",
        Platform  = "ServiceAxis",
        Timestamp = DateTime.UtcNow
    })).WithTags("Health").AllowAnonymous();

    Log.Information("ServiceAxis Platform started successfully.");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ServiceAxis Platform terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
