using Application.Models;

namespace Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryModel>> GetCategoriesAsync();
}
