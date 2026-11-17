namespace Infrastructure.Seed.Models;

public class CartSeedModel
{
    public long UserId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
}