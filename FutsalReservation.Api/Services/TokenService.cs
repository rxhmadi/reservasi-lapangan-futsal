using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FutsalReservation.Api.Models;

namespace FutsalReservation.Api.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string BuatToken(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var secret = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key belum diset");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var kredensial = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nama),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: kredensial);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
