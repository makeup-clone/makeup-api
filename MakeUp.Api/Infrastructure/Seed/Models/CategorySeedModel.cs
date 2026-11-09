namespace Infrastructure.Seed.Models;
public class CategorySeedModel
{
    public string Name { get; set; } = string.Empty;
    public string UrlSlug { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? Image { get; set; }
    public string? Description { get; set; }

    public ICollection<CategorySeedModel> Children { get; set; } = [];
}
