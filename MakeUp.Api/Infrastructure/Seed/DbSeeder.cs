using Domain.Entities.Identity;
using Infrastructure.Constants;
using Infrastructure.Seed.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAllAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DbSeeder");

        logger.LogInformation("Розпочато процес ініціалізації та сідування бази даних...");

        await SeedRolesAsync(roleManager, logger);
        await SeedUsersAsync(userManager, logger);

        // Тут у майбутньому можна БАЖАНО додати інші сідери
    }

    public static async Task SeedRolesAsync(RoleManager<RoleEntity> roleManager, ILogger logger)
    {
        if (!roleManager.Roles.Any())
        {
            foreach (var roleName in Roles.AllRoles)
            {
                var role = new RoleEntity(roleName);
                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    logger.LogInformation("Додали нову роль {RoleName}", roleName);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        logger.LogError("Проблема при створенні ролі {RoleName}: {ErrorDescription}", roleName, error.Description);
                    }
                }
            }
        }
    }

    public static async Task SeedUsersAsync(UserManager<UserEntity> userManager, ILogger logger)
    {
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = "Infrastructure.Seed.Data.Users.json";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var users = JsonSerializer.Deserialize<List<UserSeedModel>>(json);

        if (users == null || users.Count == 0)
        {
            logger.LogError("Не вдалося отримати список користувачів. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in users)
        {
            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                continue;

            var user = new UserEntity
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Image = model.Image
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Не вдалося створити користувача {Email}. Помилки: {Errors}", model.Email, errors);
                continue;
            }


            result = await userManager.AddToRolesAsync(user, model.Roles);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Не вдалося додати ролі для користувача {Email}. Помилки: {Errors}", model.Email, errors);
            }
        }
    }
}
