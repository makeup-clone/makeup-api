namespace Infrastructure.Seed.Models;

public class ProductsSeedModel
{
    public string Name { get; set; } = string.Empty;
    public string UrlSlug { get; set; } = string.Empty;

    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }

    public long BrandId { get; set; }
    public long CategoryId { get; set; }
}