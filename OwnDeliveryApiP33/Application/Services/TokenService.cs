using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(Courier courier)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(
            double.TryParse(jwtSettings["ExpiresInMinutes"], out var mins) ? mins : 60);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, courier.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, courier.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, courier.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, courier.LastName),
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
