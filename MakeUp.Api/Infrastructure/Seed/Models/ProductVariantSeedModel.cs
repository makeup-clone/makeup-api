public class ProductVariantSeedModel
{
    public string? ImageUrl { get; set; }

    public string Sku { get; set; } = string.Empty;
    public string Volume { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Quantity { get; set; }

    public long ProductId { get; set; }
}