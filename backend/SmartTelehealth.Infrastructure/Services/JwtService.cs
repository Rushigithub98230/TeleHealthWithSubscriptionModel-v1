using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartTelehealth.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "default-secret-key-for-development");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("UserId", user.Id.ToString())
        };
        
        // Add role claims if user has roles
        // Note: In a real implementation, you would get roles from the database
        claims.Add(new Claim(ClaimTypes.Role, "User"));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = _configuration["Jwt:Issuer"] ?? "SmartTelehealth",
            Audience = _configuration["Jwt:Audience"] ?? "SmartTelehealth"
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
    
    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "default-secret-key-for-development");
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "SmartTelehealth",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "SmartTelehealth",
                ValidateLifetime = false // We want to validate the token even if it's expired
            }, out SecurityToken validatedToken);
            
            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    public bool ValidateToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        return principal != null;
    }
    
    public string? GetUserIdFromToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        return principal?.FindFirst("UserId")?.Value;
    }
    
    public string? GetEmailFromToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }
} 