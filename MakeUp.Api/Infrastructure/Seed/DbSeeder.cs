using System.Text.Json;
using Domain.Entities.Identity;
using Domain.Entities;
using Infrastructure.Constants;
using Infrastructure.Seed.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAllAsync(IServiceProvider serviceProvider, AppDbContext dbContext)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DbSeeder");

        logger.LogInformation("Розпочато процес ініціалізації та сідування бази даних...");

        await SeedRolesAsync(roleManager, logger);
        await SeedUsersAsync(userManager, logger);
        await SeedCategoriesAsync(dbContext, logger);

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
  

    public static async Task SeedCategoriesAsync(AppDbContext dbContext, ILogger logger)
    {
        string jsonName = "Categories.json";
        var filePath = Path.Combine(AppContext.BaseDirectory, "Seed", "Data",jsonName);

        Console.WriteLine($"Шлях до файлу: {filePath}");

        if (!File.Exists(filePath))
        {
            logger.LogError("Файл {JsonName} не знайдено", jsonName);
            return;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var categories = JsonSerializer.Deserialize<List<CategorySeedModel>>(json, options);

        if (categories is null)
        {
            logger.LogInformation("Categories.json не має даних для seed");
            return;
        }

        foreach (var model in categories)
        {
            await AddCategoryAsync(model, dbContext, null);
        }

        logger.LogInformation("Успішно додано усі категорії");
    }

    public static async Task AddCategoryAsync(CategorySeedModel categorySeedModel, AppDbContext dbContext, long? parentId)
    {
        var category = await dbContext.Categories
                .FirstOrDefaultAsync(x => x.UrlSlug == categorySeedModel.UrlSlug);

        Console.WriteLine($"Додаємо категорію: {categorySeedModel.Name}, UrlSlug: {categorySeedModel.UrlSlug}, ParentId: {parentId}");

        if (category is null)
        {
            category = new CategoryEntity
            {
                Name = categorySeedModel.Name,
                UrlSlug = categorySeedModel.UrlSlug,
                Priority = categorySeedModel.Priority,
                Image = categorySeedModel.Image,
                Description = categorySeedModel.Description,
                ParentId = parentId
            };

            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
        }

        foreach (var child in categorySeedModel.Children)
        {
            await AddCategoryAsync(child, dbContext, category.Id);
        }
    }
}