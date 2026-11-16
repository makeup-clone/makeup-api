using Application.Interfaces;
using Application.Mappers;
using Application.Models;
using Domain.Constants;
using Domain.Entities.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Application.Services;

public class AuthService(UserManager<UserEntity> userManager, IImageService imageService, UserMapper userMapper, IJwtTokenService jwtTokenService, IConfiguration configuration) : IAuthService
{
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
        
        var token = await jwtTokenService.CreateTokenAsync(user);
        return new AuthResponseModel { AccessToken = token };
    }


    public async Task<AuthResponseModel> LoginAsync(LoginModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
            throw new Exception("Невірний email або пароль.");

        var isPassValid = await userManager.CheckPasswordAsync(user, model.Password);
        if (!isPassValid)
            throw new Exception("Невірний email або пароль.");

        var token = await jwtTokenService.CreateTokenAsync(user);
        return new AuthResponseModel { AccessToken = token };
    }


    public async Task<AuthResponseModel> GoogleLoginAsync(GoogleLoginModel model)
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

        var token = await jwtTokenService.CreateTokenAsync(user);
        return new AuthResponseModel { AccessToken = token };
    }
}
