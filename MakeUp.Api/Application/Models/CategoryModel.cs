namespace Application.Models;

public class CategoryModel
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? UrlSlug { get; set; }
    public int Priority { get; set; }
    public string? Image { get; set; }
    public string? Description { get; set; }
    public long? ParentId { get; set; }
    public List<CategoryModel> Children { get; set; } = [];
}
