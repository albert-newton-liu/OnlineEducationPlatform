using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OnlineEducation.Utils;

public interface IJwtTokenHelper
{
    string GenerateToken(string userId, int role, int expireMinutes = 60);

    (string? userId, int? role)? ParseToken(string token);
}

public class JwtTokenHelper : IJwtTokenHelper
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtTokenHelper(IConfiguration config, JwtSecurityTokenHandler tokenHandler)
    {
        _secretKey = config["Jwt:SecretKey"]!;
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
        _tokenHandler = tokenHandler;
    }

    public string GenerateToken(string userId, int role, int expireMinutes = 60)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return _tokenHandler.WriteToken(token);
    }

    public (string? userId, int? role)? ParseToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        // Use NameIdentifier instead of JwtRegisteredClaimNames.Sub
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;
        int.TryParse(roleClaim, out var role);

        return (userId, role);
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            // Ensure the claim mapping is correct
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

        try
        {
            return _tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch (SecurityTokenExpiredException)
        {
            Console.WriteLine("Token expired");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            Console.WriteLine("Invalid signature");
            return null;
        }
        catch (SecurityTokenInvalidAudienceException)
        {
            Console.WriteLine("Invalid audience");
            return null;
        }
        catch (SecurityTokenInvalidIssuerException)
        {
            Console.WriteLine("Invalid issuer");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Other validation error: {ex.Message}");
            return null;
        }
    }
}
