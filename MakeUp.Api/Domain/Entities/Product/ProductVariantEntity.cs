using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Product;

//Варіанти продукту за розміром, кольором, об'ємом тощо
[Table("tblProductVariants")]
public class ProductVariantEntity : BaseEntity<long>{

    //Image, який відображається на сторінці продукту першим, якщо у нього є кілька варіантів
    public string? ImageUrl { get; set; }

    [Required]
    public string Sku { get; set; } = string.Empty;
    public string Volume { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldPrice { get; set; }

    public int Quantity { get; set; }

    public long ProductId { get; set; }
    public virtual ProductEntity Product { get; set; } = null!;
}