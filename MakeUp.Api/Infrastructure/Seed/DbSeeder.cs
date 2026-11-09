using System.Text.Json;
using Domain.Entities.Identity;
using Domain.Entities;
using Infrastructure.Constants;
using Infrastructure.Seed.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAllAsync(IServiceProvider serviceProvider, AppDbContext dbContext)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DbSeeder");

        logger.LogInformation("Розпочато процес ініціалізації та сідування бази даних...");

        await SeedRolesAsync(roleManager, logger);
        await SeedCategoriesAsync(dbContext, logger);
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

    public static async Task SeedCategoriesAsync(AppDbContext dbContext, ILogger logger)
    {
        string jsonName = "Categories.json";
        var filePath = Path.Combine(AppContext.BaseDirectory, "Seed", "Data",jsonName);

        Console.WriteLine($"Шлях до файлу: {filePath}");

        if (!File.Exists(filePath))
        {
            logger.LogError($"Файл {jsonName} не знайдено");
            throw new FileNotFoundException($"{jsonName} not found");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var categories = JsonSerializer.Deserialize<List<CategorySeedModel>>(json);

        if (categories is null)
        {
            logger.LogInformation("Categories.json не має даних для seed");
            return;
        }

        foreach (var model in categories)
        {
            await AddCategoryAsync(model, dbContext, null);
        }

        logger.LogInformation("Успіно додано усі категорії");
    }

    public static async Task AddCategoryAsync(CategorySeedModel categorySeedModel, AppDbContext dbContext, long? parentId)
    {
        var category = await dbContext.Categories
                .FirstOrDefaultAsync(x => x.UrlSlug == categorySeedModel.UrlSlug);

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
