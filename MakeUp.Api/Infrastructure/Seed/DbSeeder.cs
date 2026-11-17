using System.Text.Json;
using Domain.Constants;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Product;
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
        var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DbSeeder");

        logger.LogInformation("Розпочато процес ініціалізації та сідування бази даних...");

        await SeedRolesAsync(roleManager, logger);
        await SeedUsersAsync(userManager, logger);
        await SeedCategoriesAsync(dbContext, logger);
        await SeedBrandsAsync(dbContext, logger);
        await SeedProductsAsync(dbContext, logger);
        await SeedFavoritesAsync(dbContext, logger);
        await SeedCartsAsync(dbContext, logger);
        await SeedReviewsAsync(dbContext, logger);

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
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = "Infrastructure.Seed.Data.Categories.json";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };  

        var categories = JsonSerializer.Deserialize<List<CategorySeedModel>>(json, options);

        if (categories == null || categories.Count == 0)
        {
            logger.LogError("Не вдалося отримати список категорій. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in categories)
            await AddCategoryAsync(model, dbContext, null);
    }

    public static async Task AddCategoryAsync(CategorySeedModel categorySeedModel, AppDbContext dbContext, long? parentId)
    {
        var category = await dbContext.Categories
                .FirstOrDefaultAsync(x => x.UrlSlug == categorySeedModel.UrlSlug);

        if (category == null)
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
            await AddCategoryAsync(child, dbContext, category.Id);
    }

    public static async Task SeedBrandsAsync(AppDbContext dbContext, ILogger logger)
    {
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = "Infrastructure.Seed.Data.Brands.json";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var brands = JsonSerializer.Deserialize<List<BrandSeedModel>>(json);

        if (brands == null || brands.Count == 0)
        {
            logger.LogError("Не вдалося отримати список брендів. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in brands)
        {
            var existingBrand = await dbContext.Brands.FirstOrDefaultAsync(b => b.Name == model.Name);
            if (existingBrand != null)
                continue;

            var brand = new BrandEntity
            {
                Name = model.Name
            };

            await dbContext.Brands.AddAsync(brand);
        }
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedProductsAsync(AppDbContext dbContext, ILogger logger)
    {
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = "Infrastructure.Seed.Data.Products.json";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var products = JsonSerializer.Deserialize<List<ProductSeedModel>>(json);

        if (products == null || products.Count == 0)
        {
            logger.LogError("Не вдалося отримати список продуктів. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in products)
        {
            var existingProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.Name == model.Name);
            if (existingProduct != null)
                continue;

            var product = new ProductEntity
            {
                Name = model.Name,
                UrlSlug = model.UrlSlug,
                AverageRating = model.AverageRating,
                ReviewsCount = model.ReviewsCount,
                BrandId = model.BrandId,
                CategoryId = model.CategoryId
            };

            await dbContext.Products.AddAsync(product);
        }
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedFavoritesAsync(AppDbContext dbContext, ILogger logger)
    {
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = "Infrastructure.Seed.Data.Favorites.json";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var favorites = JsonSerializer.Deserialize<List<FavoriteSeedModel>>(json);

        if (favorites == null || favorites.Count == 0)
        {
            logger.LogError("Не вдалося отримати список улюблених. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in favorites)
        {
            var existingFavorite = await dbContext.Favorites.FirstOrDefaultAsync(f => f.UserId == model.UserId && f.ProductId == model.ProductId);
            if (existingFavorite != null)
                continue;

            var favorite = new FavoriteEntity
            {
                UserId = model.UserId,
                ProductId = model.ProductId
            };

            await dbContext.Favorites.AddAsync(favorite);
        }
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedCartsAsync(AppDbContext dbContext, ILogger logger)
    {
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = "Infrastructure.Seed.Data.Carts.json";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var carts = JsonSerializer.Deserialize<List<CartSeedModel>>(json);

        if (carts == null || carts.Count == 0)
        {
            logger.LogError("Не вдалося отримати список кошиків. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in carts)
        {
            var existingCart = await dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == model.UserId && c.ProductId == model.ProductId);
            if (existingCart != null)
                continue;

            var cart = new CartEntity
            {
                UserId = model.UserId,
                ProductId = model.ProductId,
                Quantity = model.Quantity
            };

            await dbContext.Carts.AddAsync(cart);
        }
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedReviewsAsync(AppDbContext dbContext, ILogger logger)
    {
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = "Infrastructure.Seed.Data.Reviews.json";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var reviews = JsonSerializer.Deserialize<List<ReviewSeedModel>>(json);

        if (reviews == null || reviews.Count == 0)
        {
            logger.LogError("Не вдалося отримати список відгуків. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in reviews)
            await AddReviewAsync(model, dbContext);
    }

    public static async Task AddReviewAsync(ReviewSeedModel reviewSeedModel, AppDbContext dbContext, long? parentId = null)
    {
        var review = await dbContext.Reviews
                .FirstOrDefaultAsync(x => x.UserId == reviewSeedModel.UserId && x.ProductId == reviewSeedModel.ProductId);

        if (review == null)
        {
            review = new ReviewEntity
            {
                UserId = reviewSeedModel.UserId,
                ProductId = reviewSeedModel.ProductId,
                Rating = reviewSeedModel.Rating,
                Text = reviewSeedModel.Text,
                LikesCount = reviewSeedModel.LikesCount,
                DislikesCount = reviewSeedModel.DislikesCount,
                ParentId = reviewSeedModel.ParentId
            };

            await dbContext.Reviews.AddAsync(review);
            await dbContext.SaveChangesAsync();
        }

        foreach (var child in reviewSeedModel.Replies)
            await AddReviewAsync(child, dbContext, child.ParentId);
    }
}