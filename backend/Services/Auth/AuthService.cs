using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dotnet_test.Controllers.Contracts;
using dotnet_test.Data;
using dotnet_test.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_test.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly AuthOptions _options;

    public AuthService(AppDbContext db, IOptions<AuthOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == request.UserName.ToLower(), cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserName),
            new(ClaimTypes.NameIdentifier, user.UserName),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.PlayerId))
        {
            claims.Add(new Claim("player_id", user.PlayerId));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        user.LastLoginAt = now;
        await _db.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAt,
            UserName = user.UserName,
            Role = user.Role,
            PlayerId = user.PlayerId
        };
    }
}
