using Application.Interfaces;
using Application.Mappers;
using Application.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class AuthService(UserManager<UserEntity> userManager, IImageService imageService, UserMapper userMapper, IJwtTokenService jwtTokenService, IConfiguration configuration, IAppDbContext context) : IAuthService
{
    private async Task<string> CreateRefreshTokenAsync(long userId, CancellationToken cancellationToken = default)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(tokenBytes);
        var tokenHash = ComputeSha256Hash(rawToken);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = tokenHash,
            ExpirationDate = DateTime.UtcNow.AddDays(CookieConstants.RefreshTokenExpirationDays),
            DateCreated = DateTime.UtcNow,
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    private static string ComputeSha256Hash(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes);
    }

    public async Task<AuthResponseModel> RegisterAsync(RegisterModel model, CancellationToken cancellationToken)
    {
        var existUser = await userManager.FindByEmailAsync(model.Email);
        if (existUser != null)
            throw new Exception("Користувач із такою електронною поштою вже існує.");

        string? imageName = null;
        if (model.Image is not null)
            imageName = await imageService.UploadImageAsync(model.Image, 300, 300, cancellationToken);

        var user = userMapper.RegisterModelToUserEntity(model);
        user.Image = imageName ?? imageService.GetDefaultUserImage();

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            if (imageName != null) 
                imageService.DeleteImage(imageName);
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new Exception($"Сталася помилка при створенні нового користувача: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new Exception($"Сталася помилка при додаванні ролі: {errors}");
        }
        
        var accessToken = await jwtTokenService.CreateTokenAsync(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        return new AuthResponseModel {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }


    public async Task<AuthResponseModel> LoginAsync(LoginModel model, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
            throw new Exception("Невірний email або пароль.");

        var isPassValid = await userManager.CheckPasswordAsync(user, model.Password);
        if (!isPassValid)
            throw new Exception("Невірний email або пароль.");

        var accessToken = await jwtTokenService.CreateTokenAsync(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        return new AuthResponseModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }


    public async Task<AuthResponseModel> GoogleLoginAsync(GoogleLoginModel model, CancellationToken cancellationToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(
            model.GoogleIdToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { configuration["GoogleAuth:ClientId"] }
            });

        if (!payload.EmailVerified)
            throw new Exception("Пошта не підтверджена в Google акаунті.");

        const string provider = "Google";
        var providerKey = payload.Subject;

        var user = await userManager.FindByLoginAsync(provider, providerKey);

        if (user == null)
        {
            user = await userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = userMapper.GooglePayloadToUserEntity(payload);
                user.EmailConfirmed = true; 

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Сталася помилка при створенні нового користувача: {errors}");
                }

                var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new Exception($"Сталася помилка при додаванні ролі: {errors}");
                }
            }

            var info = new UserLoginInfo(provider, providerKey, provider);
            var addLoginResult = await userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                throw new Exception($"Не вдалося прив'язати Google-акаунт до користувача: {errors}");
            }
        }

        var accessToken = await jwtTokenService.CreateTokenAsync(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, cancellationToken);
        return new AuthResponseModel    
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponseModel> RefreshTokenAsync(string? rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            throw new UnauthorizedAccessException("Refresh-токен відсутній.");

        var tokenHash = ComputeSha256Hash(rawRefreshToken);

        var storedToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.IsDeleted)
            throw new UnauthorizedAccessException("Невалідний refresh-токен.");

        if (storedToken.ExpirationDate <= DateTime.UtcNow)
        {
            storedToken.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Термін дії refresh-токена вичерпано.");
        }

        if (storedToken.User is null || storedToken.User.IsDeleted)
            throw new UnauthorizedAccessException("Користувача не знайдено або акаунт неактивний.");

        // Позначаємо поточний refresh-токен як використаний (ротація)
        storedToken.IsDeleted = true;

        var newAccessToken = await jwtTokenService.CreateTokenAsync(storedToken.User);
        var newRefreshToken = await CreateRefreshTokenAsync(storedToken.UserId, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponseModel
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string? rawRefreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            return;

        var tokenHash = ComputeSha256Hash(rawRefreshToken);

        var storedToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == tokenHash && !rt.IsDeleted, cancellationToken);

        if (storedToken is null)
            return;

        storedToken.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }
}
