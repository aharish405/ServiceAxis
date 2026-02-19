using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Identity.Services;
using ServiceAxis.Shared.Settings;
using System.Text;

namespace ServiceAxis.Identity;

/// <summary>
/// Extension methods for registering Identity-layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind strongly-typed settings
        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()!;

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        // ── JWT Bearer authentication ──────────────────────────────────────
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(
                                              Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer           = true,
                ValidIssuer              = jwtSettings.Issuer,
                ValidateAudience         = true,
                ValidAudience            = jwtSettings.Audience,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            };
        });

        // ── Authorization policies ─────────────────────────────────────────
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",   p => p.RequireRole("SuperAdmin", "Admin"));
            options.AddPolicy("ManagerUp",   p => p.RequireRole("SuperAdmin", "Admin", "Manager"));
            options.AddPolicy("AgentUp",     p => p.RequireRole("SuperAdmin", "Admin", "Manager", "Agent"));
            options.AddPolicy("AnyAuthenticated", p => p.RequireAuthenticatedUser());
        });

        // ── Application contracts ──────────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
