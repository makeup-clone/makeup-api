using Application.Interfaces;
using Application.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class CategoryService(IAppDbContext context, IMapper mapper) : ICategoryService
{
    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        var allCategories = await context.Categories
        .OrderBy(c => c.Priority)
        .ToListAsync();

        var rootCategories = allCategories
            .Where(c => c.ParentId == null)
            .ToList();

        return mapper.Map<List<CategoryModel>>(rootCategories);
    }
}
