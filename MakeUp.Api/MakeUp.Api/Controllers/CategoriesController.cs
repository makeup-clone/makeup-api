using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace MakeUp.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoriesController(ICategoryService categoryService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CategoryModel>>> GetAll()
        {
            var categories = await categoryService.GetCategoriesAsync();
            return Ok(categories);
        }
    }
}
