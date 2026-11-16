using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Product;

[Table("tblProductImages")]
public class ProductImageEntity : BaseEntity<long>
{
    [Required, StringLength(200)]
    public string ImageUrl { get; set; } = string.Empty;
    public int Priority { get; set; }

    public long ProductId { get; set; }
    public virtual ProductEntity Product { get; set; } = null!;
}
