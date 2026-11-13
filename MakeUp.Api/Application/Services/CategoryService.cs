using Application.Interfaces;
using Application.Mappers;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class CategoryService(IAppDbContext context, CategoryMapper categoryMapper) : ICategoryService
{
    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        var allCategories = await context.Categories
        .OrderBy(c => c.Priority)
        .ToListAsync();

        var rootCategories = allCategories
            .Where(c => c.ParentId == null)
            .ToList();

        return categoryMapper.MapChildren(rootCategories);
    }
}
