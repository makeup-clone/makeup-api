using Application.Interfaces;
using Application.Mappers;
using Application.Models;
using Domain.Constants;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class AuthService(UserManager<UserEntity> userManager, IImageService imageService, UserMapper userMapper, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponceModel> RegisterAsync(RegisterModel model, CancellationToken cancellationToken)
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
        return new AuthResponceModel { AccessToken = token };
    }
}
