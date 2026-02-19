using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Shared.Exceptions;
using ServiceAxis.Shared.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities;

namespace ServiceAxis.Identity.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<IdentityUser> userManager,
        IUnitOfWork uow,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _uow = uow;
        _jwtSettings = jwtSettings.Value;
    }

    // ── Login ─────────────────────────────────────────────────────────────

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new BusinessException("Invalid credentials.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new BusinessException("Invalid credentials.");

        return await GenerateTokensAndBuildResponseAsync(user);
    }

    // ── Register ──────────────────────────────────────────────────────────

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            throw new ConflictException($"A user with email '{request.Email}' already exists.");

        var user = new IdentityUser
        {
            UserName       = request.Email,
            Email          = request.Email,
            EmailConfirmed = true  // skip email confirmation for now
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new BusinessException($"Registration failed: {string.Join("; ", errors)}");
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "Agent");

        // Create domain ApplicationUser profile
        var appUser = new ApplicationUser
        {
            IdentityUserId = user.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        await _uow.Repository<ApplicationUser>().AddAsync(appUser, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return await GenerateTokensAndBuildResponseAsync(user);
    }

    // ── Refresh ───────────────────────────────────────────────────────────

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        // Find user by stored refresh token
        // We store refresh tokens as Identity user tokens with loginProvider = "ServiceAxis"
        var allUsers = _userManager.Users.ToList();

        IdentityUser? user = null;
        foreach (var u in allUsers)
        {
            var storedToken = await _userManager.GetAuthenticationTokenAsync(
                u, "ServiceAxis", "RefreshToken");
            if (storedToken == refreshToken)
            {
                user = u;
                break;
            }
        }

        if (user is null)
            throw new BusinessException("Invalid or expired refresh token.");

        return await GenerateTokensAndBuildResponseAsync(user);
    }

    // ── Revoke ────────────────────────────────────────────────────────────

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var allUsers = _userManager.Users.ToList();
        foreach (var u in allUsers)
        {
            var stored = await _userManager.GetAuthenticationTokenAsync(u, "ServiceAxis", "RefreshToken");
            if (stored == refreshToken)
            {
                await _userManager.RemoveAuthenticationTokenAsync(u, "ServiceAxis", "RefreshToken");
                return;
            }
        }
        // Silently ignore unknown tokens (already revoked / expired)
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<AuthResponse> GenerateTokensAndBuildResponseAsync(IdentityUser user)
    {
        var roles  = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var tokenClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        tokenClaims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        tokenClaims.AddRange(claims);

        // Include TenantId if available in ApplicationUser profile
        var appUser = (await _uow.Repository<ApplicationUser>().FindAsync(u => u.IdentityUserId == user.Id)).FirstOrDefault();
        if (appUser?.TenantId != null)
        {
            tokenClaims.Add(new Claim("tenant_id", appUser.TenantId.Value.ToString()));
        }
        else if (appUser != null && !appUser.TenantId.HasValue)
        {
            // For now, if no tenant is assigned, we might want to default to a 'Default' tenant 
            // but in a real enterprise setup, this is handled during onboarding.
        }

        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer:             _jwtSettings.Issuer,
            audience:           _jwtSettings.Audience,
            claims:             tokenClaims,
            expires:            expires,
            signingCredentials: creds);

        var accessToken  = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        // Persist refresh token
        await _userManager.SetAuthenticationTokenAsync(user, "ServiceAxis", "RefreshToken", refreshToken);

        return new AuthResponse(
            AccessToken:  accessToken,
            RefreshToken: refreshToken,
            ExpiresAt:    expires,
            UserId:       user.Id,
            Email:        user.Email!,
            Roles:        roles);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
