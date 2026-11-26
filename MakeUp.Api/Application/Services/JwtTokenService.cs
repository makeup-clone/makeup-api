using Application.Interfaces;
using Domain.Constants;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services;

public class JwtTokenService(IConfiguration configuration, UserManager<UserEntity> userManager) : IJwtTokenService
{
    public async Task<string> CreateTokenAsync(UserEntity user)
    {
        // 1. Формуємо claims (дані, які будуть зашиті всередині токена)
        var claims = new List<Claim>
        {
            new ("id", user.Id.ToString()),
            new ("firstName", user.FirstName ?? string.Empty),
            new ("lastName", user.LastName ?? string.Empty),
            new ("email", user.Email ?? string.Empty),
            new ("image", user.Image ?? string.Empty),
            new ("createdAt", user.DateCreated.ToString("O"))
        };
        foreach (var role in await userManager.GetRolesAsync(user))
            claims.Add(new Claim("roles", role));

        var logins = await userManager.GetLoginsAsync(user);
        var providers = logins
            .Select(l => l.LoginProvider)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();
        foreach (var provider in providers)
            claims.Add(new Claim("providers", provider));

        // 2. Читаємо секретний ключ із appsettings.json
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Описуємо параметри токена
        var jwtSecurityToken = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(CookieConstants.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

        string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return token;
    }
}
