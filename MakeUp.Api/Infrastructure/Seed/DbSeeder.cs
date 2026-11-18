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
        await SeedReviewsAsync(dbContext, logger);
        await SeedProductAttributesAsync(dbContext, logger);
        await SeedAttributeValuesAsync(dbContext, logger);
        await SeedProductAttributeValuesAsync(dbContext, logger);
        await SeedProductVariantsAsync(dbContext, logger);
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

    public static async Task<T?> GetDeserializeJsonData<T>(string fileName, ILogger logger) where T : class
    {
        var domainAssembly = typeof(DbSeeder).Assembly;
        var jsonPath = $"Infrastructure.Seed.Data.{fileName}";
        using var stream = domainAssembly.GetManifestResourceStream(jsonPath);
        if (stream == null)
        {
            logger.LogError("Не вдалося знайти вбудований ресурс {JsonPath}. Перевірте назву шляху та Build Action файлу.", jsonPath);
            return null;
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var data = JsonSerializer.Deserialize<T>(json);

        return data;
    }


    public static async Task SeedUsersAsync(UserManager<UserEntity> userManager, ILogger logger)
    {
        var users = await GetDeserializeJsonData<List<UserSeedModel>>("Users.json", logger);

        if (users == null || users.Count == 0)
        {
            logger.LogError("Не вдалося отримати список користувачів. Список дорівнює null або являється пустим");
            return;
        }

        var existingEmails = new HashSet<string>(
            await userManager.Users.Select(u => u.Email!).ToListAsync(),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var model in users)
        {
            if (existingEmails.Contains(model.Email))
                continue;

            var user = new UserEntity
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Image = model.Image,
                EmailConfirmed = true
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

            existingEmails.Add(model.Email);
        }
    }

    public static async Task SeedCategoriesAsync(AppDbContext dbContext, ILogger logger)
    {
        var categories = await GetDeserializeJsonData<List<CategorySeedModel>>("Categories.json", logger);

        if (categories == null || categories.Count == 0)
        {
            logger.LogError("Не вдалося отримати список категорій. Список дорівнює null або являється пустим");
            return;
        }

        // Отримуємо всі існуючі UrlSlug з бази в HashSet 
        //Використовуємо HashSet для швидкого пошуку за O(1)
        var existingSlugs = new HashSet<string>(
            await dbContext.Categories.Select(c => c.UrlSlug).ToListAsync(),
            StringComparer.OrdinalIgnoreCase
        );

        var newCategories = new List<CategoryEntity>();

        foreach (var model in categories)
        {
            ExtractNewCategoriesRecursive(model, null, existingSlugs, newCategories);
        }

        if (newCategories.Count > 0)
        {
            await dbContext.Categories.AddRangeAsync(newCategories);
            await dbContext.SaveChangesAsync();
        }
    }

    private static void ExtractNewCategoriesRecursive(
        CategorySeedModel model,
        CategoryEntity? parentEntity,
        HashSet<string> existingSlugs,
        List<CategoryEntity> newCategories)
    {
        // Якщо така категорія вже є в базі, ми її пропускаємо (або можна оновлювати, але зазвичай сідер просто пропускає)
        if (existingSlugs.Contains(model.UrlSlug))
            return;

        var category = new CategoryEntity
        {
            Name = model.Name,
            UrlSlug = model.UrlSlug,
            Priority = model.Priority,
            Image = model.Image,
            Description = model.Description,
            Parent = parentEntity
        };

        newCategories.Add(category);
        existingSlugs.Add(model.UrlSlug); // Додаємо, щоб уникнути дублікатів усередині самого JSON

        foreach (var child in model.Children)
        {
            ExtractNewCategoriesRecursive(child, category, existingSlugs, newCategories);
        }
    }

    public static async Task SeedBrandsAsync(AppDbContext dbContext, ILogger logger)
    {
        var brands = await GetDeserializeJsonData<List<BrandSeedModel>>("Brands.json", logger);

        if (brands == null || brands.Count == 0)
        {
            logger.LogError("Не вдалося отримати список брендів. Список дорівнює null або являється пустим");
            return;
        }

        var existingNames = new HashSet<string>(
            await dbContext.Brands.Select(b => b.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase
        );
        var newBrands = new List<BrandEntity>();

        foreach (var model in brands)
        {
            if (existingNames.Contains(model.Name))
                continue;

            newBrands.Add(new BrandEntity
            {
                Name = model.Name
            });

            existingNames.Add(model.Name);
        }
        if (newBrands.Count > 0)
        {
            await dbContext.Brands.AddRangeAsync(newBrands);
            await dbContext.SaveChangesAsync();
        }
    }

    public static async Task SeedProductsAsync(AppDbContext dbContext, ILogger logger)
    {
        var products = await GetDeserializeJsonData<List<ProductSeedModel>>("Products.json", logger);

        if (products == null || products.Count == 0)
        {
            logger.LogError("Не вдалося отримати список продуктів. Список дорівнює null або являється пустим");
            return;
        }

        var existingNames = new HashSet<string>(
            await dbContext.Products.Select(b => b.Name).ToListAsync(),
            StringComparer.OrdinalIgnoreCase
        );
        var newProducts = new List<ProductEntity>();

        foreach (var model in products)
        {
            if (existingNames.Contains(model.Name))
                continue;

            newProducts.Add(new ProductEntity
            {
                Name = model.Name,
                UrlSlug = model.UrlSlug,
                AverageRating = model.AverageRating,
                ReviewsCount = model.ReviewsCount,
                BrandId = model.BrandId,
                CategoryId = model.CategoryId
            });

            existingNames.Add(model.Name);
        }
        if (newProducts.Count > 0)
        {
            await dbContext.Products.AddRangeAsync(newProducts);
            await dbContext.SaveChangesAsync();
        }
    }

    public static async Task SeedReviewsAsync(AppDbContext dbContext, ILogger logger)
    {
        var reviews = await GetDeserializeJsonData<List<ReviewSeedModel>>("Reviews.json", logger);

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

    public static async Task SeedProductAttributesAsync(AppDbContext dbContext, ILogger logger)
    {
        var productAttributes = await GetDeserializeJsonData<List<ProductAttributeSeedModel>>("ProductAttributes.json", logger);

        if (productAttributes == null || productAttributes.Count == 0)
        {
            logger.LogError("Не вдалося отримати список атрибутів товару. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in productAttributes)
        {
            var existingProduct = await dbContext.ProductAttributes.FirstOrDefaultAsync(p => p.Name == model.Name);
            if (existingProduct != null)
                continue;

            var productAttribute = new ProductAttributeEntity
            {
                Name = model.Name,
                Slug = model.Slug,
            };

            await dbContext.ProductAttributes.AddAsync(productAttribute);
        }
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedAttributeValuesAsync(AppDbContext dbContext, ILogger logger)
    {
        var attributeValues = await GetDeserializeJsonData<List<AttributeValueSeedModel>>("AttributeValues.json", logger);

        if (attributeValues == null || attributeValues.Count == 0)
        {
            logger.LogError("Не вдалося отримати список значень атрибутів. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in attributeValues)
        {
            var existingValue = await dbContext.AttributeValues.FirstOrDefaultAsync(v => v.ProductAttributeId == model.ProductAttributeId && v.Value == model.Value);
            if (existingValue != null)
                continue;

            var attributeValue = new AttributeValueEntity
            {
                ProductAttributeId = model.ProductAttributeId,
                Value = model.Value,
            };

            await dbContext.AttributeValues.AddAsync(attributeValue);
        }
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedProductAttributeValuesAsync(AppDbContext dbContext, ILogger logger)
    {
        var productAttributeValues = await GetDeserializeJsonData<List<ProductAttributeValueSeedModel>>("ProductAttributeValues.json", logger);

        if (productAttributeValues == null || productAttributeValues.Count == 0)
        {
            logger.LogError("Не вдалося отримати список прив'язаних значень атрибутів. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in productAttributeValues)
        {
            var existingValue = await dbContext.ProductAttributeValues.FirstOrDefaultAsync(v => v.ProductId == model.ProductId && v.AttributeValueId == model.AttributeValueId);
            if (existingValue != null)
                continue;

            var productAttributeValue = new ProductAttributeValueEntity
            {
                ProductId = model.ProductId,
                AttributeValueId = model.AttributeValueId,
            };

            await dbContext.ProductAttributeValues.AddAsync(productAttributeValue);
        }
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedProductVariantsAsync(AppDbContext dbContext, ILogger logger)
    {
        var productVariants = await GetDeserializeJsonData<List<ProductVariantSeedModel>>("ProductVariants.json", logger);

        if (productVariants == null || productVariants.Count == 0)
        {
            logger.LogError("Не вдалося отримати список варіантів продуктів. Список дорівнює null або являється пустим");
            return;
        }

        foreach (var model in productVariants)
        {
            var existingValue = await dbContext.ProductVariants.FirstOrDefaultAsync(v => v.ProductId == model.ProductId && v.Sku == model.Sku);
            if (existingValue != null)
                continue;

            var productVariant = new ProductVariantEntity
            {
                ProductId = model.ProductId,
                Sku = model.Sku,
                Volume = model.Volume,
                Price = model.Price,
                OldPrice = model.OldPrice,
                Quantity = model.Quantity
            };

            await dbContext.ProductVariants.AddAsync(productVariant);
        }
        await dbContext.SaveChangesAsync();
    }
}